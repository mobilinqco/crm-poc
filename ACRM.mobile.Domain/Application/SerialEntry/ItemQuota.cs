using System;
namespace ACRM.mobile.Domain.Application.SerialEntry
{
    public class ItemQuota
    {

        private QuotaArticleData _articleConfiguration = null;
        private QuotaData _quotaConfiguration = null;
        private int _initialCount = 0;
        private int _defaultQuotaPerYearWithoutConfiguration;
        public ItemQuota(QuotaArticleData articleConfiguration, QuotaData quotaConfiguration, int maxQuota, int initialCount)
        {

            _articleConfiguration = articleConfiguration;
            _quotaConfiguration = quotaConfiguration;
            _defaultQuotaPerYearWithoutConfiguration = maxQuota;
            _initialCount = initialCount;

        }

        public int maxQuota
        {
            get
            {
                if (_articleConfiguration == null)
                {
                    return _defaultQuotaPerYearWithoutConfiguration;
                }
                else
                {
                    return _articleConfiguration.Quota;
                }
            }

        }

        public int remainingQuota
        {
            get
            {
                return remainingQuotaForCount(0);
            }

        }
        
        public int remainingQuotaForCount(int count)
        {
            if (_quotaConfiguration != null)
            {
                return maxQuota - _quotaConfiguration.QuantityIssued + _initialCount - count;
            }
            else
            {
                return maxQuota - count;
            }

        }
        public bool unlimitedQuota
        {
            get
            {
                return _quotaConfiguration == null ? true : _quotaConfiguration.Allocated;
            }

        }

    }
}
