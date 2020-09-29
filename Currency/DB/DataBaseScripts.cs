using Platform.Disposables;
using Platform.Collections.Stacks;
using Platform.Converters;
using Platform.Memory;
using Platform.Data;
using Platform.Data.Numbers.Raw;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Unicode;
using Platform.Data.Doublets.Sequences.Walkers;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.CriterionMatchers;
using Platform.Data.Doublets.Memory.Split.Specific;
using TLinkAddress = System.UInt32;

namespace Currency
{
    //Part of the code, along with comments, is taken from https://github.com/linksplatform/Comparisons.SQLiteVSDoublets/commit/289cf361c82ab605b9ba0d1621496b3401e432f7
    public class DataBase : DisposableBase
    {
        string indexFileName;
        string dataFileName;
        private readonly TLinkAddress _meaningRoot;
        private readonly TLinkAddress _unicodeSymbolMarker;
        private readonly TLinkAddress _unicodeSequenceMarker;
        private readonly RawNumberToAddressConverter<TLinkAddress> _numberToAddressConverter;
        private readonly AddressToRawNumberConverter<TLinkAddress> _addressToNumberConverter;
        private readonly IConverter<string, TLinkAddress> _stringToUnicodeSequenceConverter;
        private readonly IConverter<TLinkAddress, string> _unicodeSequenceToStringConverter;
        private readonly ILinks<TLinkAddress> _disposableLinks;
        private readonly ILinks<TLinkAddress> links;

        public DataBase()
        {
            this.indexFileName = "indexes";
            this.dataFileName = "data.db";

            var dataMemory = new FileMappedResizableDirectMemory(this.dataFileName);
            var indexMemory = new FileMappedResizableDirectMemory(this.indexFileName);

            var linksConstants = new LinksConstants<TLinkAddress>(enableExternalReferencesSupport: true);

            // Init the links storage
            _disposableLinks = new UInt32SplitMemoryLinks(dataMemory, indexMemory, UInt32SplitMemoryLinks.DefaultLinksSizeStep, linksConstants); // Low-level logic
            links = new UInt32Links(_disposableLinks); // Main logic in the combined decorator

            // Set up constant links (markers, aka mapped links)
            TLinkAddress currentMappingLinkIndex = 1;
            _meaningRoot = GerOrCreateMeaningRoot(currentMappingLinkIndex++);
            _unicodeSymbolMarker = GetOrCreateNextMapping(currentMappingLinkIndex++);
            _unicodeSequenceMarker = GetOrCreateNextMapping(currentMappingLinkIndex++);
            // Create converters that are able to convert link's address (UInt64 value) to a raw number represented with another UInt64 value and back
            _numberToAddressConverter = new RawNumberToAddressConverter<TLinkAddress>();
            _addressToNumberConverter = new AddressToRawNumberConverter<TLinkAddress>();

            // Create converters that are able to convert string to unicode sequence stored as link and back
            var balancedVariantConverter = new BalancedVariantConverter<TLinkAddress>(links);
            var unicodeSymbolCriterionMatcher = new TargetMatcher<TLinkAddress>(links, _unicodeSymbolMarker);
            var unicodeSequenceCriterionMatcher = new TargetMatcher<TLinkAddress>(links, _unicodeSequenceMarker);
            var charToUnicodeSymbolConverter = new CharToUnicodeSymbolConverter<TLinkAddress>(links, _addressToNumberConverter, _unicodeSymbolMarker);
            var unicodeSymbolToCharConverter = new UnicodeSymbolToCharConverter<TLinkAddress>(links, _numberToAddressConverter, unicodeSymbolCriterionMatcher);
            var sequenceWalker = new RightSequenceWalker<TLinkAddress>(links, new DefaultStack<TLinkAddress>(), unicodeSymbolCriterionMatcher.IsMatched);
            _stringToUnicodeSequenceConverter = new CachingConverterDecorator<string, TLinkAddress>(new StringToUnicodeSequenceConverter<TLinkAddress>(links, charToUnicodeSymbolConverter, balancedVariantConverter, _unicodeSequenceMarker));
            _unicodeSequenceToStringConverter = new CachingConverterDecorator<TLinkAddress, string>(new UnicodeSequenceToStringConverter<TLinkAddress>(links, unicodeSequenceCriterionMatcher, sequenceWalker, unicodeSymbolToCharConverter));
        }

        private TLinkAddress GerOrCreateMeaningRoot(TLinkAddress meaningRootIndex) => links.Exists(meaningRootIndex) ? meaningRootIndex : links.CreatePoint();

        private TLinkAddress GetOrCreateNextMapping(TLinkAddress currentMappingIndex) => links.Exists(currentMappingIndex) ? currentMappingIndex : links.CreateAndUpdate(_meaningRoot, links.Constants.Itself);

        public string ConvertToString(TLinkAddress sequence) => _unicodeSequenceToStringConverter.Convert(sequence);

        public TLinkAddress ConvertToSequence(string @string) => _stringToUnicodeSequenceConverter.Convert(@string);

        public void Delete(TLinkAddress link) => links.Delete(link);

        public TLinkAddress Create(string value, string date, string charCode)
        {
            var valueLink = ConvertToSequence(value);
            var dateLink = ConvertToSequence(date);
            var charCodeLink = ConvertToSequence(charCode);
            return this.links.GetOrCreate(this.links.GetOrCreate(charCodeLink, dateLink), valueLink);
        }

        public string Each(string date, string charCode)
        {
            var currencyRatePair = this.links.SearchOrDefault(ConvertToSequence(charCode), ConvertToSequence(date));
            var currencyRateValue = "";
            if (currencyRatePair != default)
            {
                var query = new Link<TLinkAddress>(this.links.Constants.Any, currencyRatePair, this.links.Constants.Any);
                this.links.Each((link) =>
                {
                    var currencyRateValueLink = link[this.links.Constants.TargetPart];
                    currencyRateValue = ConvertToString(currencyRateValueLink);
                    return this.links.Constants.Break;
                }, query);
            }
            return currencyRateValue;
        }

        protected override void Dispose(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
            {
                _disposableLinks.DisposeIfPossible();
            }
        }
    }
}
