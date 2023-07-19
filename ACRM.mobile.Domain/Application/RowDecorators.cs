using System;
using ACRM.mobile.Domain.Configuration.UserInterface;

namespace ACRM.mobile.Domain.Application
{
    public class RowDecorators
    {
        public (string image, string glyph) Image;
        public string LeftMarginColor;
        public Expand Expand;

        public RowDecorators()
        {
        }
    }
}
