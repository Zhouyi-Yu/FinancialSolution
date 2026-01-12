import pandas as pd
import numpy as np
from datetime import datetime

# ==========================================
# 1. DATA INGESTION (Simulating Power Query)
# ==========================================
def load_and_transform_data():
    # simulating raw CSV data
    data = {
        'Date': pd.date_range(start='2024-01-01', periods=100, freq='D').tolist() * 2,
        'Merchant': ['Walmart', 'Starbucks', 'Uber', 'Shell', 'Amazon', 'Netflix'] * 33 + ['Safeway', 'Tim Hortons'],
        'Category': ['Groceries', 'Dining', 'Transport', 'Transport', 'Shopping', 'Entertainment'] * 33 + ['Groceries', 'Dining'],
        'Amount': np.random.uniform(-150, -5, 200).round(2) # Expenses are negative
    }
    df = pd.DataFrame(data)
    
    # Transformation: Add ID columns for Star Schema
    df['TransactionID'] = range(1, len(df) + 1)
    df['CategoryID'] = df.groupby('Category').ngroup() + 1
    df['MerchantID'] = df.groupby('Merchant').ngroup() + 1
    
    return df

# ==========================================
# 2. DATA MODELING (Star Schema)
# ==========================================
def build_star_schema(df):
    print("--- Building Star Schema ---")
    
    # FACT Table: Transactions
    # Contains Keys and Metrics
    fact_transactions = df[['TransactionID', 'Date', 'MerchantID', 'CategoryID', 'Amount']].copy()
    print(f"Fact_Transactions: {len(fact_transactions)} rows")
    
    # DIMENSION: Date
    dim_date = pd.DataFrame({'Date': df['Date'].unique()})
    dim_date['Month'] = dim_date['Date'].dt.month
    dim_date['Year'] = dim_date['Date'].dt.year
    dim_date['Quarter'] = dim_date['Date'].dt.quarter
    print(f"Dim_Date: {len(dim_date)} rows")

    # DIMENSION: Category
    dim_category = df[['CategoryID', 'Category']].drop_duplicates()
    # Simulating a manually entered Budget Limit
    budget_map = {'Groceries': 500, 'Dining': 200, 'Transport': 150, 'Shopping': 300, 'Entertainment': 50}
    dim_category['BudgetLimit'] = dim_category['Category'].map(budget_map).fillna(100)
    print(f"Dim_Category: {len(dim_category)} rows")

    # DIMENSION: Merchant
    dim_merchant = df[['MerchantID', 'Merchant']].drop_duplicates()
    print(f"Dim_Merchant: {len(dim_merchant)} rows")
    
    return fact_transactions, dim_date, dim_category, dim_merchant

# ==========================================
# 3. DAX MEASURE SIMULATION (Business Logic)
# ==========================================
def calculate_dax_measures(fact, dim_date, dim_cat):
    print("\n--- Calculating DAX Measures ---")
    
    # Measure: Total Spend
    # DAX: CALCULATE(SUM(Amount), Amount < 0) * -1
    total_spend = fact[fact['Amount'] < 0]['Amount'].sum() * -1
    print(f"Total Spend: ${total_spend:,.2f}")
    
    # Measure: Spend by Category
    # Simulating a Matrix Visual
    merged = fact.merge(dim_cat, on='CategoryID')
    spend_by_cat = merged[merged['Amount'] < 0].groupby('Category')['Amount'].sum().abs()
    
    print("\n[Visual] Spend by Category:")
    print(spend_by_cat)

    # Measure: Budget Utilization %
    # DAX: DIVIDE([Total Spend], SUM(BudgetLimit))
    print("\n[Visual] Budget Utilization %:")
    for cat, spend in spend_by_cat.items():
        limit = dim_cat[dim_cat['Category'] == cat]['BudgetLimit'].values[0]
        utilization = (spend / limit) * 100
        print(f"  {cat}: {utilization:.1f}% (Limit: ${limit})")

if __name__ == "__main__":
    # 1. ETL
    raw_df = load_and_transform_data()
    
    # 2. Modeling
    fact, d_date, d_cat, d_merch = build_star_schema(raw_df)
    
    # 3. Analytics
    calculate_dax_measures(fact, d_date, d_cat)
