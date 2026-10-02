# Nova

> Modernizing a custom 1980s DataFlex/MS-DOS business system for US Trek Enterprise Inc.

Nova is a desktop business management app I’m building for **US Trek Enterprise Inc.**

It is meant to replace an old custom DataFlex/MS-DOS program called **FLEX**, which was developed around 1987.

The goal is not to recreate FLEX exactly. I’m keeping the useful workflows and business data, but rebuilding the app with a modern interface and a cleaner database structure.

---

## 🧩 What Nova Handles

Right now, Nova is focused on:

- Parts
- Customers
- Vendors
- Company and part relationships
- Price history
- Basic data entry and validation

Later, I plan to add:

- Search and editing
- Quotes
- Pro formas
- Invoices
- Reports
- Data migration from FLEX

---

## 🛠️ Tech Stack

**C# · .NET · WinUI 3 · XAML · Entity Framework Core · SQLite · Git**

---

## 🔗 How the Data Is Organized

One of the biggest changes from FLEX is how parts and companies are connected.

Instead of storing a limited number of company records directly inside a part, Nova uses a relationship between them:

```text
Part
  ↕
PartCompany
  ↕
Company
```

This makes it possible for:

- one part to be connected to many companies
- one company to be connected to many parts
- each company to have its own part number
- pricing history to be stored for that specific company and part

---

## 🚧 Current Status

Nova is still in active development.

Current focus:

```text
Parts
Customers
Vendors
   ↓
Search and editing
   ↓
Quotes
   ↓
Pro formas
   ↓
Invoices
   ↓
Reports
```

---

## 🔒 Privacy

The original FLEX system contains real business data.

This repository does not include real customer, vendor, financial, or legacy database files.
