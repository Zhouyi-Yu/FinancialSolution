# Power BI Design Document: Financial Transaction Dashboard

**Project**: Financial Data Warehouse & Analytics
**Tool**: Power BI (Design & Data Modeling focus)

## 1. Data Modeling Strategy (Star Schema)

Instead of a single flat table, the data is modeled into a **Star Schema** to optimize performance and enable complex filtering.

### Fact Table
*   **Fact_Transactions**
    *   `TransactionID` (Key)
    *   `DateKey` (FK)
    *   `MerchantID` (FK)
    *   `CategoryID` (FK)
    *   `Amount` (Decimal)
    *   `Description` (Text)

### Dimension Tables
*   **Dim_Date**
    *   `DateKey`
    *   `Month`, `Quarter`, `Year`, `FiscalYear`
*   **Dim_Category**
    *   `CategoryID`
    *   `CategoryName` (e.g., "Utilities", "Groceries")
    *   `BudgetLimit` (Decimal)
*   **Dim_Merchant**
    *   `MerchantID`
    *   `MerchantName`
    *   `City`, `Region`

---

## 2. Power Query Transformations (M Code Logic)

Before loading, the raw CSV data undergoes the following ETL steps:
1.  **Promote Headers**: Use first row as headers.
2.  **Change Type**: Force `Amount` to *Fixed Decimal Number* (Currency), `Date` to *Date*.
3.  **Conditional Column**: Create `TransactionType` based on Amount (>0 is Income, <0 is Expense).
4.  **Reference Query**: Split unique Merchants into `Dim_Merchant` to normalize the dataset.

---

## 3. DAX Measures (Business Logic)

The following DAX (Data Analysis Expressions) measures were defined to calculate KPIs dynamically based on filter context (Date, Category).

### Core Financials
```dax
Total Spend = 
CALCULATE(
    SUM(Fact_Transactions[Amount]),
    Fact_Transactions[Amount] < 0
) * -1
```

```dax
Total Income = 
CALCULATE(
    SUM(Fact_Transactions[Amount]),
    Fact_Transactions[Amount] > 0
)
```

### Strategic Metrics (Time Intelligence)
```dax
// Calculate Month-over-Month Growth to spot spending trends
MoM Spend Growth % = 
VAR CurrentMonth = [Total Spend]
VAR PriorMonth = CALCULATE([Total Spend], DATEADD(Dim_Date[Date], -1, MONTH))
RETURN
    DIVIDE(CurrentMonth - PriorMonth, PriorMonth, 0)
```

### Budget Performance
```dax
// Dynamic budget utilization based on category limits
Budget Utilization % = 
DIVIDE(
    [Total Spend],
    SUM(Dim_Category[BudgetLimit]),
    0
)
```

## 4. Visualization Plan
*   **Executive Scorecard**: KPIs for *Total Spend*, *MoM Growth*, *Savings Rate*.
*   **Decomposition Tree**: Analyze *Total Spend* broken down by *Category* -> *Merchant*.
*   **Trend Line**: *Total Spend* vs *Budget* over time (Dim_Date[Month]).
