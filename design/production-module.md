# Production Module — Domain Knowledge Extracted from Bakery Operations

> **Source:** Real bakery operational experience (El Rico Pan, Matamoros).  
> **Status:** Requirements reference for Pana's future Production module.  
> **Date extracted:** 2026-07-04

---

## 1. Raw Material Costing

### Entity: RawMaterial

| Field | Type | Description |
|---|---|---|
| Name | string | e.g. "Harina de trigo 00", "Huevo blanco" |
| Category | enum | Flours, Sugars, Eggs, Dairy, Fats, Leaveners, Flavorings, Other |
| PurchaseUnit | enum | kg, g, L, mL, pza, docena, cono |
| PurchasePrice | decimal | What you paid for the package |
| YieldPct | decimal | % usable after processing (default 100) |
| PresentationQty | decimal | How many units the package contains (e.g. 500 for "500g bag") |
| BaseUnit | enum | Unit for cost calculation (g, mL, pza) |
| Supplier | string? | Optional supplier name |
| IsActive | bool | Soft delete |

### Core Formula: Effective Cost Per Base Unit

```
costPerBaseUnit = purchasePrice / (presentationQty × (yieldPct / 100))
```

**Example:**
- 500g flour bag costs $12.00
- Yield is 80% (20% processing loss — dusting, spillage)
- `costPerBaseUnit = $12.00 / (500 × 0.8) = $12.00 / 400 = $0.03/g`

**Why yield matters in a bakery:**
- Flour: ~95% (dusting loss)
- Eggs: ~90% (shell weight, breakage)
- Yeast: ~98%
- Sugar: ~99%
- Butter: ~97% (residue on wrappers)

---

## 2. Unit Conversion System

### Conversion Factors (relative to base unit = 1)

```
kg  → 1000g
g   → 1g     (base)
L   → 1000mL
mL  → 1mL    (base)
pza → 1pza   (base — countable items)
```

### Converting Between Units

```
conversionFactor = fromUnitFactor / toUnitFactor
costContribution = qty × conversionFactor × material.costPerBaseUnit
```

**Example:** Recipe calls for 200g flour, but flour is priced per kg.
- Flour costPerBaseUnit = $0.024/g (calculated from kg purchase)
- `costContribution = 200 × 1.0 × $0.024 = $4.80`

---

## 3. Recipe Costing

### Entity: Recipe

| Field | Type | Description |
|---|---|---|
| Name | string | e.g. "Concha de Vainilla" |
| Yield | decimal | How many units this recipe produces |
| YieldUnit | string | piezas, kg, docenas |
| Ingredients | List<RecipeIngredient> | Linked to RawMaterial |
| LaborCostPerUnit | decimal | $/unit (e.g. $0.50 per piece) |
| EnergyCost | decimal | Fixed gas/electricity cost per batch |
| OverheadPct | decimal | % applied on top of raw material cost |
| Scaling | 0.5x, 1x, 2x, 3x | Supported batch sizes |

### Entity: RecipeIngredient

| Field | Type | Description |
|---|---|---|
| MaterialId | Guid | FK → RawMaterial |
| Qty | decimal | Amount used |
| Unit | enum | May differ from material's purchase unit |

### Cost Breakdown Formulas

```
rawMaterialCost    = Σ (ingredient.qty × conversionFactor × material.costPerBaseUnit)
laborCost          = laborCostPerUnit × yield
overheadCost       = rawMaterialCost × (overheadPct / 100)
totalBatchCost     = rawMaterialCost + laborCost + energyCost + overheadCost
costPerUnit        = totalBatchCost / yield
```

**Example — Concha de Vainilla (24 piezas):**

| Ingredient | Qty | Unit | Cost |
|---|---|---|---|
| Harina | 500 | g | $12.00 |
| Azúcar | 200 | g | $3.60 |
| Mantequilla | 150 | g | $12.75 |
| Huevo | 4 | pza | $8.00 |
| Levadura | 15 | g | $0.90 |
| Vainilla | 10 | mL | $1.50 |
| Sal | 8 | g | $0.12 |
| **Raw material cost** | | | **$38.87** |
| Labor ($0.50/pza × 24) | | | $12.00 |
| Energy (gas batch) | | | $3.50 |
| Overhead (5% × $38.87) | | | $1.94 |
| **Total batch cost** | | | **$56.31** |
| **Cost per piece** | | | **$2.35** |

---

## 4. Production & Waste Tracking

### Daily Inventory Capture (Bakery Workflow)

Every morning, the baker records:
1. **Inicial** — Stock carried over from yesterday (unsold items)
2. **Producción** — How many were made today
3. **Devolución** — Returns/waste at end of day

```
Available = Inicial + Producción
Sold = Available - Devolución - EndOfDayStock
WasteRate = Devolución / (Inicial + Producción) × 100
Efficiency = (Producción - Devolución) / Producción × 100
```

### Waste Categories (Return Reasons)

| Category | Subcategories |
|---|---|
| Quemado | Horno, Descuido |
| Deformado | Masa, Manipulación |
| Sobrante | Sobreproducción, Baja demanda |
| Caducidad | Tiempo en vitrina |
| Manipulación | Caída, transporte |
| Calidad | Sabor, textura, apariencia |
| Otro | Especificar |

### Waste Cost Formula

```
costoMerma = Σ (mermaPorProducto × costoPorUnidad)
```

Where `costoPorUnidad` comes from the recipe calculation.

---

## 5. Daily Context

Factors that affect bakery sales — useful for correlation analysis:

| Field | Values | Description |
|---|---|---|
| Weather | frio, templado, calor, lluvia | Weather affects foot traffic and product preference |
| DayType | normal, finde, festivo, vacaciones | Weekend/holiday patterns |
| IsPayday | bool | Payday effect (quinzena in Mexico: 15th and 30th) |
| HasLocalEvent | bool | Fairs, sports events, school events |
| SchoolOut | bool | School vacation periods |
| LowStock | bool | Were any products out of stock today? |

---

## 6. BCG Matrix (Business Analytics)

Products are classified on two axes:
- **X-axis:** Relative market share (within the bakery's own portfolio)
- **Y-axis:** Sales growth rate (period over period)

| Quadrant | Strategy |
|---|---|
| **Stars** ⭐ | High growth + high share → Invest, promote |
| **Cash Cows** 🐄 | Low growth + high share → Maintain, milk profits |
| **Question Marks** ❓ | High growth + low share → Evaluate, decide |
| **Dogs** 🐕 | Low growth + low share → Phase out or reposition |

### Algorithm (Simplified)

```
avgSales = average units sold across all products in period
avgGrowth = average growth rate across all products

for each product:
    relativeShare = product.salesInPeriod / avgSales
    growthRate = (product.salesThisPeriod - product.salesLastPeriod) / product.salesLastPeriod
    
    if growthRate >= avgGrowth AND relativeShare >= 1.0 → Star
    if growthRate <  avgGrowth AND relativeShare >= 1.0 → Cash Cow
    if growthRate >= avgGrowth AND relativeShare <  1.0 → Question Mark
    if growthRate <  avgGrowth AND relativeShare <  1.0 → Dog
```

---

## 7. Notes for Pana Implementation

- **RawMaterial** maps naturally to Pana's existing `Product` entity (type = Consumable) — but dedicated entity may be cleaner for domain clarity
- **Recipe** is a new module — not yet in Pana. This is the #1 gap.
- **WasteCategory** can enrich Pana's `InventoryMovement` (add `WasteCategoryId` FK)
- **DailyContext** is a standalone entity — easy ~30min add, huge analytical value
- **BCG Matrix** should be a read-only query/view, not a domain entity — materialized PostgreSQL view
- The unit conversion logic can live in a `UnitConversionService` utility, not as a separate module
