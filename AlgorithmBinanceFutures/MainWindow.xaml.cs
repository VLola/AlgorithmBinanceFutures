using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Binance.Net.Enums;
using Binance.Net.Objects.Spot.MarketData;
using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace AlgorithmBinanceFutures
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<History> history = new List<History>();
        public List<ListKlines> LIST_KLINES = new List<ListKlines>();
        public MainWindow()
        {
            InitializeComponent();
            ErrorWatcher();
            ListAlgorithm();
            FilesList();
            HistoryList.ItemsSource = history;
        }
        // ------------------------------------------------------- Start Login Block -------------------------------------------
        private void Button_Login(object sender, RoutedEventArgs e)
        {
            Socket.Connect(api_key.Text, secret_key.Text);
        }
        // ------------------------------------------------------- End Login Block ---------------------------------------------
        // ------------------------------------------------------- Start Error Text Block --------------------------------------
        private void ErrorWatcher()
        {
            FileSystemWatcher error_watcher = new FileSystemWatcher();
            error_watcher.Path = ErrorText.Directory();
            error_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            error_watcher.Changed += new FileSystemEventHandler(OnChanged);
            error_watcher.Filter = ErrorText.Patch();
            error_watcher.EnableRaisingEvents = true;
        }
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => { error_log.Text = File.ReadAllText(ErrorText.FullPatch()); }));
        }
        private void Button_ClearErrors(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(ErrorText.FullPatch(), "");
        }
        // ------------------------------------------------------- End Error Text Block ----------------------------------------
        // ------------------------------------------------------- Start USDT Bet ----------------------------------------------
        public decimal USDT;
        private void usdt_bet_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (usdt_bet.Text.Length > 0)
            {
                USDT = Decimal.Parse(usdt_bet.Text);
                if (USDT > 0m && TAKE_PROFIT > 0m) win_bet.Text = (COEFICIENT * TAKE_PROFIT * USDT).ToString();
                if (USDT > 0m && STOP_LOSS > 0m) lose_bet.Text = (COEFICIENT * STOP_LOSS * USDT).ToString();
            }
        }
        // ------------------------------------------------------- End USDT Bet ------------------------------------------------
        // ------------------------------------------------------- Start (Take Profit, Stop Loss) ------------------------------
        public decimal TAKE_PROFIT;
        public decimal STOP_LOSS;
        public decimal LOSE_BET;
        public decimal COEFICIENT = 0.01m;
        private void take_profit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (take_profit.Text.Length > 0)
            {
                TAKE_PROFIT = Decimal.Parse(take_profit.Text);
                if (USDT > 0m && TAKE_PROFIT > 0m) win_bet.Text = (COEFICIENT * TAKE_PROFIT * USDT).ToString();
            }
        }
        private void stop_loss_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (stop_loss.Text.Length > 0)
            {
                STOP_LOSS = Decimal.Parse(stop_loss.Text);
                if (USDT > 0m && STOP_LOSS > 0m)
                {
                    LOSE_BET = COEFICIENT * STOP_LOSS * USDT;
                    lose_bet.Text = LOSE_BET.ToString();
                }
            }
        }
        // ------------------------------------------------------- End (Take Profit, Stop Loss) --------------------------------
        // ------------------------------------------------------- Start Digit Check TextBox -----------------------------------
        private void digit_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !((Char.IsDigit(e.Text, 0) || ((e.Text == System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0].ToString()) && (DS_Count(((TextBox)sender).Text) < 1))));
        }
        public int DS_Count(string s)
        {
            string substr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0].ToString();
            int count = (s.Length - s.Replace(substr, "").Length) / substr.Length;
            return count;
        }
        // ------------------------------------------------------- End Digit Check TextBox --------------------------------------
        // ------------------------------------------------------- Start Algorithm ----------------------------------------------
        public void Algorithm()
        {
            try
            {
                List<string> sort = new List<string>();
                foreach (var it in ListSymbols())
                {
                    sort.Add(it.Symbol);
                }
                sort.Sort();

                int count = Convert.ToInt32(klines_end.Text);
                if (current_time.IsChecked.Value == true)
                {
                    foreach (var symbol in sort)
                    {
                        Klines(symbol, klines_count: count);
                    }
                    WriteToFile(LIST_KLINES);
                }
                else
                {
                    DateTime TIME = data_picker.SelectedDate.Value.Date;
                    if (StartTime.IsChecked.Value == true)
                    {
                        TIME = TIME.AddHours(Double.Parse(start_time_h.Text));
                        TIME = TIME.AddMinutes(Double.Parse(start_time_m.Text));
                    }
                    else
                    {
                        TIME = TIME.AddHours(Double.Parse(end_time_h.Text));
                        TIME = TIME.AddMinutes(Double.Parse(end_time_m.Text));
                    }


                    foreach (var symbol in sort)
                    {
                        if (StartTime.IsChecked.Value == true) Klines(symbol, start_time: TIME, klines_count: count);
                        else Klines(symbol, end_time: TIME, klines_count: count);
                    }
                    WriteToFile(LIST_KLINES);
                }

            }
            catch (Exception e)
            {
                ErrorText.Add($"Algorithm {e.Message}");
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Algorithm();
        }
        // ------------------------------------------------------- End Algorithm -------------------------------------------------
        // ------------------------------------------------------- Start Klines Block --------------------------------------------
        public int KLINE_START;
        public int KLINE_END;

        private void klines_start_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (klines_start.Text.Length > 0 && klines_start.Text.Length < 4)
            {
                KLINE_START = Convert.ToInt32(klines_start.Text);
            }
        }

        private void klines_end_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (klines_end.Text.Length > 0 && klines_end.Text.Length < 4)
            {
                KLINE_END = Convert.ToInt32(klines_end.Text);
            }
        }
        public void Klines(string SYMBOL, DateTime? start_time = null, DateTime? end_time = null, int? klines_count = null)
        {
            try
            {

                var result = Socket.futures.Market.GetKlinesAsync(symbol: SYMBOL, interval: KlineInterval.OneMinute, startTime: start_time, endTime: end_time, limit: klines_count).Result;
                if (!result.Success) ErrorText.Add("Error GetKlinesAsync");
                else
                {
                    List<Kline> list = new List<Kline>();
                    foreach (var it in result.Data.ToList())
                    {
                        list.Add(new Kline(it.OpenTime, it.Open, it.High, it.Low, it.Close, it.CloseTime));
                    }
                    LIST_KLINES.Add(new ListKlines(SYMBOL, list));
                }
            }

            catch (Exception e)
            {
                ErrorText.Add($"Klines {e.Message}");
            }
        }

        public void WriteToFile(List<ListKlines> list)
        {
            try
            {
                int size = list[0].listKlines.Count();
                string klines = size.ToString();
                DateTime time_start = list[0].listKlines[0].OpenTime;
                DateTime time_end = list[0].listKlines[size - 1].OpenTime;
                string path_date_start = $"{time_start.Year}.{time_start.Month}.{time_start.Day}_{time_start.Hour}.{time_start.Minute}-";
                string path_date_end = $"{time_end.Year}.{time_end.Month}.{time_end.Day}_{time_end.Hour}.{time_end.Minute}-";
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "");
                string path_full = @"\times\" + path_date_start + path_date_end + klines + ".txt";
                string json = JsonConvert.SerializeObject(list);
                File.AppendAllText(path + path_full, json);
                ErrorText.Add("Готово");
                LIST_KLINES.Clear();
                FilesList();
            }
            catch (Exception e)
            {
                ErrorText.Add($"WriteToFile {e.Message}");
            }
        }
        // ------------------------------------------------------- End Klines Block ----------------------------------------------
        // ------------------------------------------------------- Start List Symbols Block --------------------------------------
        public List<BinancePrice> ListSymbols()
        {
            try
            {
                var result = Socket.futures.Market.GetPricesAsync().Result;
                if (!result.Success) ErrorText.Add("Error GetKlinesAsync");
                return result.Data.ToList();
            }
            catch (Exception e)
            {
                ErrorText.Add($"ListSymbols {e.Message}");
                return ListSymbols();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (history.Count > 0) history.Clear();
            WriteHistory();
        }

        // ------------------------------------------------------- End List Symbols Block --------------------------------------
        // ------------------------------------------------------- Start Box List ------------------------------------------------
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
        // Start List Algorithm ------------------------------------------------------------------------------------------------
        public void ListAlgorithm()
        {
            boxList list_algorithm = new boxList();
            cmbTest.ItemsSource = list_algorithm.cmbContent;
            cmbTest.SelectedItem = list_algorithm.cmbContent[0];
        }
        // End List Algorithm --------------------------------------------------------------------------------------------------
        // ------------------------------------------------------- End Box List ------------------------------------------------
        // ------------------------------------------------------- Start Texts List --------------------------------------------
        public void FilesList()
        {
            try
            {
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "times");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                List<string> list = new List<string>();
                List<string> filesDir = (from a in Directory.GetFiles(path) select System.IO.Path.GetFileNameWithoutExtension(a)).ToList();
                if (filesDir.Count > 0)
                {
                    boxList file_list = new boxList(filesDir);
                    cmbTest1.ItemsSource = file_list.cmbContentFileNames;
                    cmbTest1.SelectedItem = file_list.cmbContentFileNames[0];
                }
            }
            catch (Exception e)
            {
                ErrorText.Add(e.Message);
            }
        }
        // ------------------------------------------------------- End Texts List ----------------------------------------------
        // ------------------------------------------------------- Start Write History Block -----------------------------------
        public decimal balance_history;
        public decimal take_profit_history;
        public decimal stop_loss_history;
        public decimal price_history;
        public string symbol_history;
        public void WriteHistory()
        {
            try
            {
                decimal takeProfit = Decimal.Parse(take_profit.Text);
                decimal stopLoss = Decimal.Parse(stop_loss.Text);
                string path = System.IO.Path.Combine(Environment.CurrentDirectory, "");
                string json = File.ReadAllText(path + @"\times\" + cmbTest1.Text + ".txt");
                var list = JsonConvert.DeserializeObject<List<ListKlines>>(json);
                PositionSide position = PositionSide.Long;
                decimal X_MULL = Decimal.Parse(x_mull.Text);
                decimal X_COUNT = Decimal.Parse(x_count.Text);
                decimal USDT = Decimal.Parse(usdt_bet.Text);
                decimal WIN_BET = Decimal.Parse(win_bet.Text);
                decimal LOSE_BET = Decimal.Parse(lose_bet.Text);
                decimal COMMISSION = USDT / 100m * 0.08m;
                foreach (var it in list)
                {
                    decimal balance_history = Decimal.Parse(balance_full.Text);
                    int PriceUp = 0;
                    int PriceDown = 0;
                    int BetUp = 0;
                    int BetDown = 0;
                    int Indefined = 0;
                    int WinUp = 0;
                    int WinDown = 0;
                    symbol_history = it.symbol;
                    int i = 0;
                    decimal X = 1m;
                    foreach (var prices_symbol in it.listKlines)
                    {
                        if (i >= KLINE_START && i <= KLINE_END)
                        {
                            if (balance_history > 0m)
                            {

                                if (price_history == 0m)
                                {
                                    price_history = prices_symbol.Open;
                                    position = Position();
                                    if (X > X_COUNT) X = 1m;
                                    if (position == PositionSide.Long)
                                    {
                                        take_profit_history = price_history + price_history / 100m * takeProfit * Convert.ToDecimal(Math.Pow(Convert.ToDouble(X_MULL), Convert.ToDouble(X)));
                                        stop_loss_history = price_history - price_history / 100m * stopLoss * Convert.ToDecimal(Math.Pow(Convert.ToDouble(X_MULL), Convert.ToDouble(X)));
                                    }
                                    else
                                    {
                                        take_profit_history = price_history - price_history / 100m * takeProfit * Convert.ToDecimal(Math.Pow(Convert.ToDouble(X_MULL), Convert.ToDouble(X)));
                                        stop_loss_history = price_history + price_history / 100m * stopLoss * Convert.ToDecimal(Math.Pow(Convert.ToDouble(X_MULL), Convert.ToDouble(X)));
                                    }

                                }
                                if (position == PositionSide.Long)
                                {
                                    if (prices_symbol.High > take_profit_history && prices_symbol.Low < stop_loss_history)
                                    {
                                        price_history = 0;
                                        Indefined++;
                                    }
                                    else if (prices_symbol.High > take_profit_history)
                                    {
                                        price_history = 0;
                                        PriceUp++;
                                        BetUp++;
                                        WinUp++;
                                        balance_history += WIN_BET;
                                        balance_history -= COMMISSION;
                                        X = 1m;
                                    }
                                    else if (prices_symbol.Low < stop_loss_history)
                                    {
                                        price_history = 0;
                                        PriceDown++;
                                        BetUp++;
                                        balance_history -= LOSE_BET;
                                        balance_history -= COMMISSION;
                                        X += 1m;
                                    }
                                }
                                else
                                {
                                    if (prices_symbol.High > stop_loss_history && prices_symbol.Low < take_profit_history)
                                    {
                                        price_history = 0;
                                        Indefined++;
                                    }
                                    else if (prices_symbol.High > stop_loss_history)
                                    {
                                        price_history = 0;
                                        PriceUp++;
                                        BetDown++;
                                        balance_history -= LOSE_BET;
                                        balance_history -= COMMISSION;
                                        X += 1m;
                                    }
                                    else if (prices_symbol.Low < take_profit_history)
                                    {
                                        price_history = 0;
                                        PriceDown++;
                                        BetDown++;
                                        WinDown++;
                                        balance_history += WIN_BET;
                                        balance_history -= COMMISSION;
                                        X = 1m;
                                    }
                                }
                            }
                            else break;
                        }
                        i++;
                    }
                    history.Add(new History(symbol_history, balance_history, PriceUp, PriceDown, BetUp, BetDown, Indefined, WinUp, WinDown));
                }
                HistoryList.Items.Refresh();
            }
            catch (Exception e)
            {
                ErrorText.Add($"WriteHistory {e.Message}");
            }
        }
        // ------------------------------------------------------- End Write History Block -------------------------------------
        // ------------------------------------------------------- Start Position Side -----------------------------------------
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            {
                return random.Next(min, max);
            }
        }
        public PositionSide Position()
        {
            try
            {
                if (cmbTest.Text == "Long" || cmbTest.Text == "Short")
                {
                    if (cmbTest.Text == "Long") return PositionSide.Long;
                    else return PositionSide.Short;
                }
                else
                {
                    if (RandomNumber(1, 3) == 1) return PositionSide.Long;
                    else return PositionSide.Short;
                }
            }
            catch (Exception e)
            {
                ErrorText.Add($"Position {e.Message}");
                return Position();
            }
        }

        // ------------------------------------------------------- End Position Side ------------------------------------------------

    }
}
