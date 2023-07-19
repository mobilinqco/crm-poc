using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ACRM.mobile.Domain.Application;

namespace ACRM.mobile.ViewModels.Base
{
    public class UITabContent
    {
        public ObservableCollection<UIWidget> Widgets { get; set; }
        public bool IsOnlyTab { get; set; } = false;
        public string Title { get; set; }
    }
}

