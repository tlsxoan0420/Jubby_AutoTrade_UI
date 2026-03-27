using System;
using System.Data;
using System.Data.SQLite; // 방금 설치한 NuGet 패키지
using System.IO;

namespace Jubby_AutoTrade_UI.DATABASE
{
    public class DB_Manager
    {
        // 🔥 파이썬 코드에 적혀있던 그 절대 경로와 똑같이 맞춰줍니다.
        private string dbPath = @"C:\Users\atrjk\OneDrive\바탕 화면\Program\04.Taemoo\Jubby Project\jubby_shared.db";
        private string connectionString;

        public DB_Manager()
        {
            // 💡 [핵심 마법의 주문]
            // Read Only=True: C#은 무조건 "읽기만" 합니다. 쓰기는 파이썬 몫!
            // Journal Mode=Wal: 파이썬이 데이터를 쓰는 찰나에 C#이 읽어도 DB가 잠기지(Locked) 않게 해줍니다.
            connectionString = $"Data Source={dbPath};Version=3;Read Only=True;Journal Mode=Wal;";
        }

        /// <summary>
        /// [공통] 쿼리를 던지면 표(DataTable) 형태로 깔끔하게 포장해서 가져오는 함수
        /// </summary>
        private DataTable GetDataTable(string query)
        {
            DataTable dt = new DataTable();
            try
            {
                // 파이썬이 아직 DB를 안 만들었다면 빈 표를 반환해서 에러 방지
                if (!File.Exists(dbPath)) return dt;

                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    {
                        using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dt); // DB 데이터를 C#의 DataTable 엑셀 표로 변환
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // 통신이 꼬여서 찰나의 에러가 나더라도 프로그램이 튕기지 않고 무시합니다.
                Console.WriteLine($"DB Read Error: {ex.Message}");
            }
            return dt;
        }

        // ====================================================================
        // 📊 C# UI의 각 표(DataGridView)에 밀어넣을 데이터 가져오기
        // ====================================================================

        // 💡 [마법의 번역기 AS 설명]
        // 파이썬은 소문자(last_price)로 DB에 저장했지만, C# 화면의 표 뼈대 이름은 대문자(Last_Price)입니다.
        // 'SELECT 파이썬이름 AS C#이름' 문법을 쓰면 C# 화면 표의 DataPropertyName과 이름이 완벽히 일치하게 되어
        // 데이터가 표의 각 칸과 차트에 쏙쏙 예쁘게 들어갑니다!

        // 1. 실시간 시장 감시 데이터 (Market Table 용)
        public DataTable GetMarketStatus()
        {
            return GetDataTable("SELECT symbol AS Symbol, symbol_name AS Name, last_price AS Last_Price, open_price AS Open_Price, high_price AS High_Price, low_price AS Low_Price, return_1m AS Return_1m, trade_amount AS Trade_Amount, vol_energy AS Vol_Energy, disparity AS Disparity, volume AS Volume FROM MarketStatus");
        }

        // 2. 실시간 계좌 잔고 데이터 (Account Table 용)
        public DataTable GetAccountStatus()
        {
            return GetDataTable("SELECT symbol AS Symbol, symbol_name AS Name, quantity AS Quantity, avg_price AS Avg_Price, current_price AS Current_Price, pnl_amt AS Pnl, pnl_rate AS Pnl_Rate, available_cash AS Available_Cash FROM AccountStatus");
        }

        // 3. 전략 분석 신호 데이터 (Strategy Table 용)
        public DataTable GetStrategyStatus()
        {
            return GetDataTable("SELECT symbol AS Symbol, symbol_name AS Name, ma_5 AS Ma_5, ma_20 AS Ma_20, RSI AS RSI, macd AS MACD, signal AS Signal FROM StrategyStatus");
        }

        // 4. 매매 체결 영수증 데이터 (Order Table 용) - 최신 500개만
        public DataTable GetTradeHistory()
        {
            // 최신 거래가 맨 위로 오도록 내림차순(DESC) 정렬
            return GetDataTable("SELECT symbol AS Symbol, symbol_name AS Name, order_type AS Order_Type, order_price AS Order_Price, order_quantity AS Order_Quantity, filled_quantity AS Filled_Quqntity, order_time AS Order_Time, Status AS Status, order_yield AS Order_Yield FROM TradeHistory ORDER BY id DESC LIMIT 500");
        }

        // ====================================================================
        // ⚙️ 시스템 상태 및 로그 가져오기
        // ====================================================================

        // 5. 현재 설정값 (상단 UI의 '현재 모드: 국내/미국' 표기용)
        public DataTable GetSharedSettings()
        {
            // 설정이나 상태창은 특정 이름에 맞춰넣을 필요 없이 그대로 가져와서 파싱하므로 SELECT * 를 씁니다.
            return GetDataTable("SELECT * FROM SharedSettings");
        }

        // 6. 파이썬 작업 진행률 (상태바 표기용)
        public DataTable GetSystemStatus()
        {
            return GetDataTable("SELECT * FROM SystemStatus");
        }

        // 7. 실시간 로그 (우측 로그 리스트박스 용) - 최신 100개만
        public DataTable GetSharedLogs()
        {
            return GetDataTable("SELECT * FROM SharedLogs ORDER BY id ASC LIMIT 100");
        }
    }
}