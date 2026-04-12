using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite; // 방금 설치한 NuGet 패키지
using System.IO;

namespace Jubby_AutoTrade_UI.DATABASE
{
    public class DB_Manager
    {
        // 🔥 하드코딩된 절대 경로 삭제! 이제 프로그램이 스스로 경로를 찾아서 여기에 넣습니다.
        private string dbPath;
        private string connectionString;

        public DB_Manager()
        {
            // 💡 1. 프로그램이 켜질 때 똑똑하게 DB 위치를 찾아냅니다.
            dbPath = GetSmartDbPath();

            // 💡 2. 찾아낸 경로를 연결 문자열에 쏙 집어넣습니다.
            // C#에서도 DB 데이터를 직접 수정(Edit)하고 저장할 수 있도록 'Read Only=True' 옵션을 삭제했습니다!
            // Journal Mode=Wal 옵션은 유지하여 파이썬과 충돌(Locked)을 방지합니다.
            connectionString = $"Data Source={dbPath};Version=3;Journal Mode=Wal;";
        }

        // =========================================================================
        // 📂 [핵심 마법 추가] 파이썬과 완벽하게 동일한 위치의 DB를 찾아내는 함수
        // =========================================================================
        private string GetSmartDbPath()
        {
            string dbName = "jubby_shared.db";
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. 빌드된 EXE 상태일 때: 현재 폴더에 DB가 있는지 먼저 확인 (최우선)
            string directPath = Path.Combine(currentDir, dbName);
            if (File.Exists(directPath))
            {
                return directPath;
            }

            // 2. Visual Studio 개발(디버그) 중일 때: bin/Debug/net... 폴더에 갇혀있으므로
            // 파이썬이 만들어둔 DB가 나올 때까지 상위 폴더로 계속 거슬러 올라가며 탐색합니다.
            DirectoryInfo dirInfo = new DirectoryInfo(currentDir);
            while (dirInfo.Parent != null)
            {
                dirInfo = dirInfo.Parent;
                string searchPath = Path.Combine(dirInfo.FullName, dbName);

                if (File.Exists(searchPath))
                {
                    return searchPath; // 파이썬이 만든 진짜 DB 발견!
                }
            }

            // 3. 정 못 찾았다면 일단 현재 폴더를 반환 (최초 실행 시 튕김 방지)
            return directPath;
        }

        // =========================================================================

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
        // 🔥 [새로 추가] DB 전체 열람 및 수정을 위한 기능들 (FormDataBase 화면 용)
        // ====================================================================

        // 1. DB 안에 있는 모든 테이블 이름 가져오기
        public List<string> GetAllTableNames()
        {
            List<string> tables = new List<string>();
            try
            {
                if (!File.Exists(dbPath)) return tables;
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    // sqlite_master에서 시스템 테이블을 제외하고 진짜 테이블 이름만 쏙쏙 뽑아옵니다.
                    string query = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%'";
                    using (SQLiteCommand cmd = new SQLiteCommand(query, conn))
                    using (SQLiteDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tables.Add(reader["name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine($"Table List Error: {ex.Message}"); }
            return tables;
        }

        // 2. 선택한 테이블의 모든 데이터 원본 가져오기 (수정용)
        public DataTable GetRawTableData(string tableName)
        {
            // C# 화면명(AS) 변환 없이 날것 그대로 가져옵니다.
            return GetDataTable($"SELECT * FROM {tableName}");
        }

        // 3. 수정한 엑셀표(DataTable)를 통째로 DB에 덮어쓰기 저장!
        public bool UpdateTableData(string tableName, DataTable modifiedData)
        {
            try
            {
                using (SQLiteConnection conn = new SQLiteConnection(connectionString))
                {
                    conn.Open();
                    // 저장할 땐 SELECT * 쿼리를 기반으로 CommandBuilder가 알아서 INSERT, UPDATE, DELETE SQL문을 만들어줍니다.
                    using (SQLiteDataAdapter adapter = new SQLiteDataAdapter($"SELECT * FROM {tableName}", conn))
                    using (SQLiteCommandBuilder builder = new SQLiteCommandBuilder(adapter))
                    {
                        adapter.Update(modifiedData);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DB Update Error: {ex.Message}");
                return false;
            }
        }

        // ====================================================================
        // 📊 C# UI의 각 표(DataGridView)에 밀어넣을 기존 데이터 가져오기
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
            // 🔥 [버그 수정] AccountStatus 테이블에는 time 컬럼이 없으므로 제외해야 에러가 발생하지 않습니다!
            string query = "SELECT symbol AS Symbol, symbol_name AS Name, quantity AS Quantity, avg_price AS Avg_Price, current_price AS Current_Price, pnl_amt AS Pnl, pnl_rate AS Pnl_Rate, available_cash AS Available_Cash FROM AccountStatus";
            return GetDataTable(query);
        }

        // 3. 전략 분석 신호 데이터 (Strategy Table 용)
        public DataTable GetStrategyStatus()
        {
            return GetDataTable("SELECT symbol AS Symbol, symbol_name AS Name, ai_prob AS AI_Prob, ma_5 AS Ma_5, ma_20 AS Ma_20, RSI AS RSI, macd AS MACD, signal AS Signal FROM StrategyStatus");
        }

        // 4. 매매 체결 영수증 데이터 (Order Table 용) - 최신 500개만
        public DataTable GetTradeHistory()
        {
            // 🔥 [버그 완벽 수정] 파이썬 DB에 저장된 진짜 컬럼명(price, time)으로 수정하여 에러를 막았습니다!
            string query = "SELECT symbol AS Symbol, symbol_name AS Name, type AS Order_Type, price AS Order_Price, quantity AS Order_Quantity, filled_quantity AS Filled_Quantity, time AS Order_Time, Status AS Status, order_yield AS Order_Yield FROM TradeHistory ORDER BY id DESC LIMIT 500";

            return GetDataTable(query);
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