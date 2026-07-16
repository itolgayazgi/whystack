# WhyStack — Kapsam Katmanı v1
### Hat içindeki alt bölümleme: EF Core, Dapper, Transaction... nasıl derinleşir?

## Sorun ve ilke

"Veri Erişimi" hattındaki EF Core tek durak olamaz — üstün körü olur. Ama EF Core'u da ayrı bir menü seviyesi yaparsak hiyerarşi 6 katmana çıkar ve kullanıcı kaybolur:

```
Alan → Ekosistem → Hat → EF Core → Alt konu → İçerik   ❌ (menü cehennemi)
```

**İlke: Derinlik navigasyonla değil, üç mekanizmayla sağlanır.** Kapsam bir menü seviyesi DEĞİL, durakları gruplayan bir etikettir:

```
Alan → Ekosistem → Hat → Durak → Blok        ← navigasyon (değişmedi, hâlâ 4 tık)
                    │
                    └── Kapsam = durak grubu  ← metadata (haritada mahalle gibi görünür)
```

---

## Derinliğin 3 mekanizması

### 1. Kapsam = Durak grubu (mahalle)
Büyük bir kapsam tek durak olmaz, **ardışık durak dizisi** olur. EF Core bir durak değil, hattın üzerinde 8 duraklık bir mahalledir — üstelik zone'lara yayılır (Junior'da 2, Mid'de 4, Senior'da 2 durağı vardır). Haritada mahalle, durakların üstünde ince bir köşeli parantezle gösterilir:

```
        ┌────────── EF CORE ──────────┐        ┌─ TRANSACTION ─┐
──●──●──●─────●─────●─────●─────◉─────●────●───●───────●────────●──
  SQL     İlk    Migr.  İlişki Track. N+1  Dapper  İzolasyon  Lock
  temel  sorgu                                                
```

**Kural: Bir kapsam 3-10 durak.** 3'ten azsa kapsam değildir (durağın kendisidir), 10'dan fazlaysa ikiye bölünür.

### 2. Durak dizisi (bir konunun bölümleri)
Tek bir konu bile 20-25 dakikaya sığmıyorsa, o konu **numaralı durak zinciri** olur:

- Change Tracking I — Neden Var? *(Kavram, 15 dk)*
- Change Tracking II — Snapshot Mekanizması *(Mekanizma, 20 dk)*
- Change Tracking III — Production'da Bellek Faciası *(Production Olayı, 15 dk)*

Önkoşul zinciri (I → II → III) zaten veri modelinde var (`prerequisites`). Yani "üstün körü vermeme"nin cevabı: **konuyu sıkıştırma, duraklara böl.** 45 dakikalık tek dev sayfa yerine 3 tamamlanabilir durak — mobil kullanım ve streak psikolojisi için de doğrusu bu.

### 3. Terim sözlüğü (durak olmayı hak etmeyen bilgi)
5 dakikadan kısa anlatılabilen mikro kavram (örn. "connection string", "DbSet nedir") durak olmaz — `term` bloğu olur: durak içinde altı kesikli çizgili kelime, tıklayınca sözlük kartı açılır. Sözlük zamanla kendi başına aranabilir bir varlığa dönüşür (SEO için de değerli).

### Boyut kuralları (özet)
| Bilginin boyu | Nereye gider |
|---|---|
| < 5 dk | `term` (sözlük kartı) |
| 5-25 dk | 1 durak |
| 25-60 dk | Durak dizisi (I, II, III...) |
| 3-10 durak eden bütün | Kapsam (mahalle etiketi) |
| > 10 durak | Kapsam ikiye bölünür |

---

## Tam örnek: B3 — Veri Erişimi hattı, kapsamlarıyla

**5 kapsam, 21 durak** (önceki matristeki 21 sayısının içi dolduruldu):

### Kapsam 3.1 — SQL & Sorgu Temelleri (3 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | ORM'den Önce: Veritabanıyla Çıplak Konuşmak | Kavram |
| Jr | Index Neden Var? Telefonun Rehberi | Kavram |
| Mid | Execution Plan Okumak — Sorgunun Röntgeni | Mekanizma |

### Kapsam 3.2 — EF Core (8 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | İlk Sorgu: EF Core Nasıl SQL Üretir? | Mekanizma |
| Jr | İlişkiler: Navigation Property'nin Bedeli | Kavram |
| Mid | Change Tracking I — Neden Var? | Kavram |
| Mid | Change Tracking II — Snapshot Mekanizması | Mekanizma |
| Mid | Include vs Select — Projection Ne Zaman Kurtarır? | Karşılaştırma |
| Mid | N+1 Problemi — Log'da 400 Sorgu | Production Olayı |
| Sr | Query Splitting ve Kartezyen Patlama | Mekanizma |
| Sr | Interceptor ve Global Query Filter | Atölye |

### Kapsam 3.3 — Micro-ORM & Ham SQL (3 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Mid | Dapper Neden Var? ORM'in Yetmediği An | Kavram |
| Mid | EF Core vs Dapper — Aynı Projede İkisi | Karşılaştırma |
| Sr | Bulk İşlemler: 100 Bin Satırı Kim Taşır? | Atölye |

### Kapsam 3.4 — Transaction & Eşzamanlılık (4 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | Transaction Neden Var? Ya Hep Ya Hiç | Kavram |
| Mid | İzolasyon Seviyeleri — Kirli Okuma Vakası | Mekanizma |
| Sr | Optimistic vs Pessimistic Locking | Karşılaştırma |
| Sr | Deadlock — İki Sorgunun Ölüm Kucaklaşması | Production Olayı |

### Kapsam 3.5 — Şema & Yaşam Döngüsü (3 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | Migration Neden Var? Şemanın Git Geçmişi | Kavram |
| Mid | Production'da Migration — Geri Dönüşü Olmayan Yol | Production Olayı |
| Exp | Sıfır Kesintili Şema Değişimi (Expand/Contract) | Atölye |

> Not: Bu hat Expert'te zayıf görünüyor (1 durak) çünkü veri erişiminin gerçek uç konuları Database ALANININ hattlarına aittir (sorgu planı optimizasyonu, partitioning...). Alan sınırı böyle korunur: B3 "uygulamadan veriye bakış"tır, Database alanı "verinin kendisi"dir. İkisi arasında ⇄ aktarma bolca olur.

---

## Veri modeline etkisi (minimal)

`Topics` tablosuna iki alan eklenir, başka hiçbir şey değişmez:

```jsonc
{
  "topicId": "dotnet-b3-ef-change-tracking-1",
  "line": "b3-data-access",          // hat (bölüm)
  "scope": "ef-core",                // KAPSAM (yeni) — gruplama etiketi
  "sequence": { "group": "change-tracking", "part": 1, "of": 3 },  // durak dizisi (yeni, ops.)
  "level": "mid",
  ...
}
```

- Harita render'ı `scope`'a bakarak mahalle parantezini çizer.
- Konu listesi ekranı `scope`'a göre gruplu (accordion) listeler.
- `sequence` varsa durak adının yanına "I / II / III" rozeti gelir ve dizi tamamlanınca tek bir "kapsam tamamlandı" kutlaması tetiklenir.

## UI'da görünüm (mevcut ekranlara ekleme)

- **Tam haritada:** kapsamlar, hat üzerinde ince altın parantez + küçük etiket (EF CORE, TRANSACTION...). Tıklayınca harita o mahalleye zoom yapar.
- **Hat listesinde (mobil Hattım):** duraklar kapsam başlıkları altında gruplanır; başlıkta "4/8" ilerleme sayacı.
- **Durak künyesinde (sağ panel):** "Kapsam: EF Core · 5/8 durak" satırı eklenir.
- **Profilde:** "Kapsam rozetleri" — bir kapsamın tüm durakları bitince kazanılır (EF Core ✓). Seviye rozetinden küçük, durak tikinden büyük bir kutlama birimi — tam doğru dozda oyunlaştırma.
