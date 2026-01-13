import pdfplumber
import os

pdf_path = "/Users/jookenblue/Desktop/ZhouyiPortofolio/projects/FinancialSolution/data_samples/sample_bill.pdf"

with pdfplumber.open(pdf_path) as pdf:
    for i, page in enumerate(pdf.pages):
        print(f"--- Page {i+1} ---")
        text = page.extract_text()
        if text:
            print(text[:1000]) # First 1000 chars
        
        tables = page.extract_tables()
        if tables:
            print(f"Found {len(tables)} tables on page {i+1}")
            for j, table in enumerate(tables):
                print(f"Table {j+1} snippet:")
                for row in table[:5]:
                    print(row)
        print("\n" + "="*50 + "\n")
