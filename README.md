# StockPortfolio

台灣股票資產庫存追蹤工具，使用 [富果行情 API](https://developer.fugle.tw/) 取得即時與歷史股價。

## 功能

- 新增持股（股票代號、股數、購入成本、購入日期）
- 即時顯示現在股價、現值、損益、報酬率（漲紅跌綠）
- 總資產摘要（總成本、總現值、總損益、總報酬率）
- 歷史走勢圖（30天 / 90天 / 半年 / 一年），顯示購入成本基準線

## 技術架構

三層式架構（Three-Tier Architecture）：

```
StockPortfolio.Domain          # 領域層：StockHolding 實體
StockPortfolio.Application     # 應用層：介面定義、DTOs
StockPortfolio.Infrastructure  # 基礎建設層：Fugle API、JSON 儲存
StockPortfolio (Web)           # 表現層：ASP.NET Core MVC
```

**技術棧：** .NET 8、ASP.NET Core MVC、Bootstrap 5、Chart.js

## 快速開始

### 1. 取得富果 API Key

前往 [developer.fugle.tw](https://developer.fugle.tw/) 註冊並取得 API Key。

### 2. 設定 API Key

在 `StockPortfolio/` 目錄下建立 `appsettings.Development.json`：

```json
{
  "Fugle": {
    "ApiKey": "你的 API Key"
  }
}
```

### 3. 執行

```bash
dotnet run --project StockPortfolio
```

瀏覽器開啟 `https://localhost:{port}`

## 注意事項

- `appsettings.Development.json` 與 `App_Data/` 已加入 `.gitignore`，不會被上傳
- 持股資料存於本機 `App_Data/portfolio.json`
- 富果 API 歷史資料查詢範圍限制為不超過一年
