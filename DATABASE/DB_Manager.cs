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

        // 1. 실시간 시장 감시 데이터 (Market Table 용)
        public DataTable GetMarketStatus()
        {
            return GetDataTable("SELECT * FROM MarketStatus");
        }

        // 2. 실시간 계좌 잔고 데이터 (Account Table 용)
        public DataTable GetAccountStatus()
        {
            return GetDataTable("SELECT * FROM AccountStatus");
        }

        // 3. 전략 분석 신호 데이터 (Strategy Table 용)
        public DataTable GetStrategyStatus()
        {
            return GetDataTable("SELECT * FROM StrategyStatus");
        }

        // 4. 매매 체결 영수증 데이터 (Order Table 용) - 최신 500개만
        public DataTable GetTradeHistory()
        {
            // 최신 거래가 맨 위로 오도록 내림차순(DESC) 정렬
            return GetDataTable("SELECT * FROM TradeHistory ORDER BY id DESC LIMIT 500");
        }

        // ====================================================================
        // ⚙️ 시스템 상태 및 로그 가져오기
        // ====================================================================

        // 5. 현재 설정값 (상단 UI의 '현재 모드: 국내/미국' 표기용)
        public DataTable GetSharedSettings()
        {
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