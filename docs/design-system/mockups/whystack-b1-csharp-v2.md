# WhyStack — B1 Genişletilmiş: C# Dili v2.1
### "Dil & Runtime" hattının tam C# müfredatı olarak açılımı (.NET ekosistemi) — 9 kapsam, 57 durak

## Yapısal cevap: Yeni katman gerekmiyor

C# başlı başına büyük bir konu — doğru. Ama bunun için yapıya yeni bir şey eklemek gerekmiyor, çünkü mevcut model tam bunu taşımak için kuruldu:

```
Backend → .NET → B1 Dil & Runtime → [9 kapsam] → [57 durak] → bloklar
                  └── .NET'te B1 = C# + CLR'dir
```

- **B1 zaten "C# hattı"dır.** Java ekosistemi seçilince aynı hat Java dilinin müfredatı olur, Go seçilince Go'nun. Hat adı ("Dil & Runtime") ekosistem-bağımsız, içeriği ekosistemin ta kendisi.
- **Derinlik kapsamlardan gelir.** 24 durak yetmiyorsa kapsam sayısını ve durak sayısını artırırız — kural belli: kapsam = 3-10 durak, 10'u aşan kapsam bölünür.
- **Önemli istisna notu:** B1, tüm hatlar içinde ekosisteme EN bağımlı hattır (~%90 ekosistem-özel; kıyasla B4 Mimari ~%80 kavram-ortak). Yani Java açıldığında B1 neredeyse sıfırdan yazılır — bu normaldir ve concept-layer maliyet hesabına böyle girmeli.

B1 böylece 24 → **57 durağa** çıkar ve haritanın en uzun hattı olur. Ana hat altın renkte zaten — en uzun hattın ana hat olması metroda da böyledir, doğru hiyerarşi.

---

## B1 — C# Dili: 9 Kapsam, 57 Durak

### Kapsam 1.1 — Tip Sistemi (7 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | Değer Tipi vs Referans Tipi — Neden İki Dünya Var? | Kavram |
| Jr | Stack ve Heap — Değişkenin Yaşadığı Yer | Mekanizma |
| Jr | null Neden Milyar Dolarlık Hata? (NRT) | Kavram |
| Mid | Boxing/Unboxing — Görünmez Maliyet | Production Olayı |
| Mid | Generics ve Kısıtlamalar — Tip Güvenliği Bedava mı? | Kavram |
| Mid | record, struct, class — Hangisi Ne Zaman? | Karşılaştırma |
| Sr | Covariance & Contravariance — in/out Neden Var? | Mekanizma |

### Kapsam 1.2 — Dil Özellikleri (7 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | Property Neden Var? Field'ın Yetmediği An | Kavram |
| Jr | Extension Method — Başkasının Sınıfına Metot Eklemek | Kavram |
| Mid | Delegate, Event, Lambda — Metodu Elden Ele Taşımak | Mekanizma |
| Mid | Pattern Matching — switch'in İkinci Hayatı | Kavram |
| Mid | yield return — Tembel Üretimin Sözdizimi | Mekanizma |
| Sr | Expression Tree — Kodun Veri Hâli | Mekanizma |
| Exp | Implicit Operatörler ve Örtük Dönüşüm Tuzakları | Production Olayı |

### Kapsam 1.3 — Koleksiyonlar & LINQ (7 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | List İçeride Nasıl Büyür? (Capacity) | Mekanizma |
| Jr | Dictionary — Hash'in Vaadi ve Bedeli | Mekanizma |
| Jr | IEnumerable — Tüm Koleksiyonların Sözleşmesi | Kavram |
| Mid | LINQ Deferred Execution — Sorgu Ne Zaman Çalışır? | Mekanizma |
| Mid | IEnumerable vs IQueryable — Sorgu Nerede Çalışır? | Karşılaştırma |
| Mid | Equality — Equals, GetHashCode ve Sözleşmenin Bozulması | Production Olayı |
| Sr | Immutable & Frozen Koleksiyonlar — Ne Zaman Değer? | Karşılaştırma |

### Kapsam 1.4 — Async & Eşzamanlılık (8 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | Thread Nedir? İşletim Sisteminin Hediyesi | Kavram |
| Jr | Senkron Dünyanın Duvarı — Neden Async? | Kavram |
| Mid | async/await Gerçekte Ne Yapar? | Mekanizma |
| Mid | ConfigureAwait ve Context — Continuation Nerede Koşar? | Mekanizma |
| Mid | Task vs ValueTask — Ne Zaman Değer? | Karşılaştırma |
| Sr | Lock, Semaphore, Channel — Eşzamanlılık Araç Seçimi | Karşılaştırma |
| Sr | Thread Pool Starvation — CPU %10'dayken Donan API | Production Olayı |
| Exp | Interlocked ve Lock-Free — Kilitsiz Dünyanın Kuralları | Mekanizma |

### Kapsam 1.5 — Bellek & GC (7 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | Garbage Collector'a Güvenmek — İlk Tanışma | Kavram |
| Mid | IDisposable ve using — GC'nin Görmediği Kaynaklar | Kavram |
| Mid | Finalizer Neden Son Çare? | Mekanizma |
| Mid | Span&lt;T&gt; ve Memory&lt;T&gt; — Kopyasız Dünya | Mekanizma |
| Sr | GC Derinlemesine: Nesiller, LOH, Gen2 Fırtınası | Mekanizma |
| Sr | Allocation Avcılığı — Profiler'da Bellek İzi Sürmek | Atölye |
| Exp | stackalloc ve ArrayPool — Heap'e Uğramadan | Atölye |

### Kapsam 1.6 — Hata Yönetimi (5 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Jr | Exception Neden Var? try/catch'in Bedeli | Kavram |
| Jr | throw vs throw ex — Stack Trace'i Kim Öldürdü? | Production Olayı |
| Mid | Custom Exception ve Exception Filter | Kavram |
| Mid | Result Pattern vs Exception — Akış Kontrolü Tartışması | Karşılaştırma |
| Sr | Exception'ın Gerçek Maliyeti — Happy Path Neden Kutsal? | Mekanizma |

### Kapsam 1.7 — Runtime & Derleme (6 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Mid | IL Nedir? C#'ın Ara Dili | Kavram |
| Mid | Assembly, AppDomain'in Mirası ve Yükleme | Mekanizma |
| Sr | JIT ve Tiered Compilation — Kodun İkinci Hayatı | Mekanizma |
| Sr | Struct Layout ve CPU Cache — Verinin Fiziği | Mekanizma |
| Exp | Native AOT ve Trimming — Ne Zaman, Neye Mal Olur? | Karşılaştırma |
| Exp | unsafe ve Interop — Sınırın Ötesi | Mekanizma |

### Kapsam 1.8 — Metaprogramlama (4 durak)
| Zone | Durak | Arketip |
|---|---|---|
| Mid | Attribute — Koda İliştirilen Bilgi | Kavram |
| Sr | Reflection'ın Gerçek Maliyeti | Production Olayı |
| Exp | Source Generator — Derleme Zamanında Kod Yazmak | Atölye |
| Exp | Expression Compile — Reflection'sız Dinamizm | Atölye |

### Kapsam 1.9 — Metin, Sayı, Zaman & Serileştirme (6 durak)
*Production hatalarının klasikleri — Türkiye'de geliştiren herkesin başına gelmiş aile.*

| Zone | Durak | Arketip |
|---|---|---|
| Jr | string Neden Immutable? StringBuilder'ın Var Olma Sebebi | Kavram |
| Jr | decimal vs double — Kuruşlar Nereye Kayboldu? | Production Olayı |
| Mid | Encoding — Türkçe Karakter Neden Bozuldu? | Production Olayı |
| Mid | DateTime vs DateTimeOffset — Saat Dilimi Faciaları | Production Olayı |
| Mid | Culture — ToString()'in Gizli Parametresi (tr-TR'de İ/i sorunu) | Mekanizma |
| Mid | System.Text.Json — Serileştirmenin Kuralları ve Maliyeti | Mekanizma |

> Not: Bu kapsam Instagram içeriği için de altın damar — kuruş kayması, İ/i sorunu ve encoding bozulması, "Production'da Ne Oldu?" serisinin doğal malzemesi.

---

## Kapsama kuralı: "Neden Testi" (durak mı, değil mi?)

B1'in hedefi C# ansiklopedisi olmak DEĞİLDİR — Microsoft Learn zaten eksiksiz referans. WhyStack'in vaadi "neden"dir. Bir C# konusunun durak olup olmayacağına şu test karar verir:

> **Bir konu, hakkında sorulacak anlamlı bir "neden" sorusu VE bir production sonucu varsa durak olur. Yoksa olmaz.**

Üç kademeli kapsama:
1. **Durak** — "neden testi"ni geçenler (bu dokümandaki 57 durak).
2. **Terim sözlüğü** — 5 dakikalık mikro bilgi (`nameof`, `params`, string interpolation, `init` accessor, `goto`...): durak değil, durak içinde tıklanabilir `term` kartı. "Tüm C#" hissini zamanla sözlük tamamlar.
3. **Bilinçli dışarıda** — saf syntax referansı: durak içinden Microsoft Learn'e bağlantı verilir. Bu zayıflık değil, güven işaretidir.

**Dil yaşıyor:** C# her yıl sürüm alır. "Tümü girildi mi?" sorusunun kalıcı cevabı yoktur; olan şey mekanizmadır — yeni dil özelliği "neden testi"ni geçiyorsa yeni durak olur ve canlı içerik döngüsü ("mahallene yeni durak eklendi" bildirimi) tamamlamış kullanıcıları geri çağırır.

---

## Seviye dağılımı (B1 v2.1)

| | Junior | Mid | Senior | Expert | Toplam |
|---|---|---|---|---|---|
| Durak | 14 | 23 | 13 | 7 | **57** |

Dağılım felsefeyle uyumlu: Mid en kalabalık (geçiş bölgesi), Expert az ve keskin.

## Backend matrisine etkisi

B1: 24 → 57 durak (9 kapsam). Backend toplamı: 154 → **~187**. Diğer hatlar değişmedi. İleride aynı büyütme B2/B3'e de uygulanabilir ama şart değil — B1'in şişkin olması doğaldır: dil, her şeyin altındaki zemindir.

## Harita ve önkoşullara etkisi

- B1 haritada 8 kapsam parantezli en uzun hat olur; tam haritada kapsamlar zoom seviyesinde görünür (uzaktan hat, yakınlaşınca mahalleler).
- Kapsamlar arası önkoşul zinciri: 1.1 Tip Sistemi → 1.3 Koleksiyonlar → 1.4 Async → 1.5 Bellek doğal sırası; 1.2 ve 1.6 paralel girilebilir; 1.7-1.8 Sr/Exp kapısı.
- Diğer hatlar B1'e önkoşulla bağlanır: B3'ün "Change Tracking" durağı ← 1.3 "Equality" durağını ister (EF Core'un entity karşılaştırması tam bu konu).

## Üretim notu

51 durak da tek seferde yazılmaz. B1 içinde bile faz mantığı işler: MVP'de 1.1 + 1.4'ün Jr/Mid durakları (12-14 durak) yeter; kalan kapsamlar haritada 🔔 ile görünür. "C# öğreniyorum" diye gelen kullanıcının ilk 3 haftasını dolduran içerik, lansman için yeterli C# derinliğidir.
