using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmBinanceFutures
{
    public class History
    {
        public string Symbol { get; set; }
        public decimal Balance { get; set; }
        public int PriceUp { get; set; }
        public int PriceDown { get; set; }
        public int BetUp { get; set; }
        public int BetDown { get; set; }
        public int Indefined { get; set; }
        public int WinUp { get; set; }
        public int WinDown { get; set; }
        public double WinLoss { get; set; }
        public History(string Symbol, decimal Balance, int PriceUp, int PriceDown, int BetUp, int BetDown, int Indefined, int WinUp, int WinDown)
        {
            this.Symbol = Symbol;
            this.Balance = Balance;
            this.PriceUp = PriceUp;
            this.PriceDown = PriceDown;
            this.BetUp = BetUp;
            this.BetDown = BetDown;
            this.Indefined = Indefined;
            this.WinUp = WinUp;
            this.WinDown = WinDown;
            WinLoss = Math.Round(Double.Parse((WinUp + WinDown).ToString()) / Double.Parse((PriceUp + PriceDown - (WinUp + WinDown)).ToString()), 2);
        }
    }
}
