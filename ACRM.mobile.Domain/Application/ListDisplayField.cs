using System;
namespace ACRM.mobile.Domain.Application
{
    public class ListDisplayField
    {
        public CrmFieldDisplayData Data { get; set; }
        public CrmFieldConfiguration Config { get; set; }

        public CrmFieldEditData EditData = new CrmFieldEditData();

        public ListDisplayField parentLdf = null;
        public int ColspanCreationFieldCounter = 0;

        public bool HasLinkButton()
        {
            if(Config.PresentationFieldAttributes.Hyperlink
                || Config.PresentationFieldAttributes.Phone
                || Config.PresentationFieldAttributes.Email)
            {
                return true;
            }

            return false;
        }

        public bool HasTime()
        {
            if (Data.ColspanData != null)
            {
                foreach (var ldf in Data.ColspanData)
                {
                    if (ldf.Config.PresentationFieldAttributes.IsTime)
                    {
                        return true;
                    }
                }
            }

            return Config.PresentationFieldAttributes.IsTime;
        }


        public bool HasDate()
        {
            if (Data.ColspanData != null)
            {
                foreach (var ldf in Data.ColspanData)
                {
                    if (ldf.Config.PresentationFieldAttributes.IsDate)
                    {
                        return true;
                    }
                }
            }

            return Config.PresentationFieldAttributes.IsDate;
        }

        public bool IsMandatoryDataReady()
        {
            if (Config.PresentationFieldAttributes.Must
                        && !EditData.HasData)
            {
                return false;
            }
            return true;
        }
    }
}
