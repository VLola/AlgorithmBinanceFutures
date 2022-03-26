using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmBinanceFutures
{
    public class ListKlines
    {
        public string symbol { get; set; }
        public List<Kline> listKlines { get; set; }
        public ListKlines(string symbol, List<Kline> listKlines)
        {
            this.symbol = symbol;
            this.listKlines = listKlines;
        }

    }

}
