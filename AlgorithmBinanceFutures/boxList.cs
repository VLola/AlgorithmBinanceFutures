using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace AlgorithmBinanceFutures
{
    public class boxList
    {
        public ObservableCollection<string> cmbContent { get; set; }
        public ObservableCollection<string> cmbContentFileNames { get; set; }
        public boxList()
        {
            cmbContent = new ObservableCollection<string>();
            cmbContent.Add("Random");
            cmbContent.Add("Long");
            cmbContent.Add("Short");
        }
        public boxList(List<string> list)
        {
            cmbContentFileNames = new ObservableCollection<string>();
            foreach (var it in list)
            {
                cmbContentFileNames.Add(it);
            }
        }
    }
}
