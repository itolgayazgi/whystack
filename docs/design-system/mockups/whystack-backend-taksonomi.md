# WhyStack — Backend İçerik Taksonomisi v1
### Bölümler, seviyeler ve konu sayıları (.NET hattı referans alınarak)

## Yapısal karar: Bölüm = Hat

Şu ana kadar metro metaforunda "her ekosistem bir hat"tı. Backend'i bölümlere ayırınca metafor daha da güçleniyor:

```
Alan (Backend)
 └── Ekosistem (.NET)  ← üst sekmelerden seçilir, ağın tamamını değiştirir
      └── Bölüm = HAT (8 renkli hat)      ← haritadaki çizgiler
           └── Konu = DURAK
                └── Basamak = ZONE (Junior → Expert, soldan sağa)
```

Yani .NET seçiliyken harita 8 hatlı bir ağ olur; Java açıldığında aynı 8 hatlı ağın Java versiyonu gelir. Kavram katmanı ekosistem-bağımsız olduğu için hat isimleri ve çoğu durak başlığı ekosistemler arasında ortak kalır — sadece kod/implementasyon slice'ları değişir. (Mevcut içerik mimarisi kararınla birebir uyumlu.)

---

## 8 Bölüm (Hat) — Backend

| # | Hat | Kapsam | Önerilen hat rengi |
|---|---|---|---|
| B1 | **Dil & Runtime** | C#, CLR, bellek, async, tip sistemi | Altın (ana hat) |
| B2 | **Web API & Framework** | ASP.NET Core, HTTP, middleware, minimal API | Turuncu |
| B3 | **Veri Erişimi** | EF Core, Dapper, transaction, migration | Mavi |
| B4 | **Mimari & Tasarım** | Clean Arch, DDD, CQRS, pattern'lar | Mor |
| B5 | **Mesajlaşma & Dağıtık Sistemler** | RabbitMQ, MassTransit, outbox, saga | Yeşil |
| B6 | **Güvenlik & Kimlik** | JWT, OAuth/OIDC, OWASP, secrets | Kırmızı |
| B7 | **Test & Kalite** | Unit/integration, mock, TDD, CI'da test | Turkuaz |
| B8 | **Performans & Gözlemlenebilirlik** | Profiling, caching, logging, metrics, tracing | Bakır |

8 sayısının gerekçesi: 6'nın altı "Mimari"yi şişirip çorbaya çevirir; 10'un üstü harita okunabilirliğini öldürür ve içerik üretimini dağıtır.

---

## Konu sayısı matrisi (tam ağ hedefi)

| Hat | Junior | Mid | Senior | Expert | Toplam |
|---|---|---|---|---|---|
| B1 Dil & Runtime | 8 | 7 | 5 | 4 | **24** |
| B2 Web API & Framework | 7 | 7 | 5 | 3 | **22** |
| B3 Veri Erişimi | 6 | 7 | 5 | 3 | **21** |
| B4 Mimari & Tasarım | 4 | 7 | 7 | 4 | **22** |
| B5 Mesajlaşma & Dağıtık | 2 | 5 | 6 | 4 | **17** |
| B6 Güvenlik & Kimlik | 4 | 5 | 4 | 2 | **15** |
| B7 Test & Kalite | 4 | 5 | 4 | 2 | **15** |
| B8 Performans & Gözlem. | 3 | 5 | 6 | 4 | **18** |
| **Toplam** | **38** | **48** | **42** | **26** | **154** |

Dağılımın mantığı:
- **Junior** temel hatlarda (B1, B2, B3) yoğun; B5 gibi dağıtık konularda neredeyse yok (junior'a saga anlatılmaz).
- **Mid** en kalabalık seviye — hedef kitlenin ağırlık merkezi burası ("developer geldin, mühendis devam et" tam bu geçiş).
- **Senior** mimari, dağıtık sistem ve performansa kayar.
- **Expert** az ama derin: her hatta 2-4 "uç" durak. Expert'i şişirmek kaliteyi düşürür.

---

## Örnek: B1 — Dil & Runtime hattının TAM durak listesi (şablon)

### Zone 1 · Junior (8 durak)
1. Değer Tipi vs Referans Tipi — Neden İki Dünya Var? *(Kavram)*
2. Stack ve Heap — Değişkenin Yaşadığı Yer *(Mekanizma)*
3. null Neden Milyar Dolarlık Hata? (Nullable Reference Types) *(Kavram)*
4. Exception Neden Var? try/catch'in Bedeli *(Kavram)*
5. Interface Neden Var? Soyutlamanın İlk Adımı *(Kavram)*
6. Delegate ve Event — Metodu Elden Ele Taşımak *(Mekanizma)*
7. LINQ Gerçekte Ne Yapar? (Deferred Execution) *(Mekanizma)*
8. Garbage Collector'a Güvenmek — İlk Tanışma *(Kavram)*

### Zone 2 · Mid (7 durak)
9. **async/await Gerçekte Ne Yapar?** *(Mekanizma — tasarımdaki örnek durak)*
10. ConfigureAwait ve Context — Continuation Nerede Koşar? *(Mekanizma)*
11. IEnumerable vs IQueryable — Sorgu Nerede Çalışır? *(Karşılaştırma)*
12. Generics ve Kısıtlamalar — Tip Güvenliğinin Bedava Olmadığı Yer *(Kavram)*
13. Span&lt;T&gt; ve Memory&lt;T&gt; — Kopyasız Dünya *(Mekanizma)*
14. Boxing/Unboxing — Görünmez Maliyet *(Production Olayı)*
15. record, struct, class — Hangisi Ne Zaman? *(Karşılaştırma)*

### Zone 3 · Senior (5 durak)
16. GC Derinlemesine: Nesil, LOH ve Gen2 Fırtınası *(Mekanizma)*
17. Thread Pool Starvation — CPU %10'dayken Donan API *(Production Olayı)*
18. Lock, Semaphore, Channel — Eşzamanlılık Silah Kataloğu *(Karşılaştırma)*
19. IDisposable ve Kaynak Sızıntıları *(Production Olayı)*
20. Reflection'ın Gerçek Maliyeti *(Mekanizma)*

### Zone 4 · Expert (4 durak)
21. Source Generator'lar — Derleme Zamanında Kod Yazmak *(Atölye)*
22. Yüksek Performanslı C#: Allocation Avcılığı *(Atölye)*
23. JIT ve Tiered Compilation — Kodun İkinci Hayatı *(Mekanizma)*
24. Unsafe ve Interop — Sınırın Ötesi *(Mekanizma)*

> Bu liste diğer 7 hattın nasıl doldurulacağının şablonudur: her durak bir "neden" sorusuyla adlandırılır, arketipi baştan bellidir, zone içinde önkoşul zinciri kurulur.

---

## Diğer hatlardan örnek duraklar (kısaltılmış)

**B2 Web API:** HTTP Gerçekte Nedir? (Jr) → Middleware Pipeline (Jr) → Model Binding'in Perde Arkası (Mid) → Rate Limiting Neden Var? (Mid) → API Versiyonlama Stratejileri (Sr) → Kestrel İç Yapısı (Exp)

**B3 Veri Erişimi:** İlk Sorgu: EF Core Nasıl SQL Üretir? (Jr) → Migration Neden Var? (Jr) → Change Tracking Neden Pahalı? (Mid) → N+1 Problemi (Mid, Production Olayı) → Transaction İzolasyon Seviyeleri (Sr) → Sorgu Planı Okumak (Exp)

**B4 Mimari:** Katman Neden Var? (Jr) → Dependency Injection Neden Var? (Mid) → CQRS'e Giriş (Mid) → DDD: Aggregate Sınırı Çizmek (Sr) → Outbox Pattern (Sr) → Event Sourcing — Ne Zaman Değmez? (Exp)

**B5 Mesajlaşma:** Queue Neden Var? (Jr) → RabbitMQ: Exchange Tipleri (Mid) → Idempotency — Aynı Mesaj İki Kez Gelirse? (Mid) → Saga Pattern (Sr) → Exactly-Once Yalanı (Exp)

**B6 Güvenlik:** Hash vs Encrypt (Jr) → JWT Gerçekte Ne Taşır? (Jr) → OAuth2 Akışları (Mid) → OWASP Top 10'u Kendi API'nde Bulmak (Sr, Atölye)

**B7 Test:** İlk Unit Test — Neyi Test Ediyoruz? (Jr) → Mock Neden Var, Ne Zaman Zarar? (Mid) → Integration Test: Testcontainers (Mid) → Test Piramidi Gerçekten Piramit mi? (Sr)

**B8 Performans:** Caching Neden Var? (Jr) → Redis: Cache Invalidation (Mid) → Distributed Tracing (Sr) → Profiler'la Canlı Avlanma (Exp, Atölye)

---

## Aktarma (⇄) noktaları — hatlar arası kesişimler

Bölümler hat olunca aktarmalar Backend'in kendi içinde de çalışır:

- B1 "Thread Pool Starvation" ⇄ B8 "Thread Metrikleri"
- B3 "Change Tracking" ⇄ B8 "Redis Cache Invalidation"
- B4 "Outbox Pattern" ⇄ B5 "Idempotency"
- B2 "Rate Limiting" ⇄ B6 "OWASP: DoS"
- B4 "CQRS" ⇄ B3 "Read Model / Projection"

Her hatta ortalama 2-3 aktarma durağı hedefle — daha fazlası haritayı karıştırır.

---

## Lansman planı (gerçekçi üretim sırası)

| Faz | Kapsam | Konu | Not |
|---|---|---|---|
| **Faz 1 — MVP** | B1 + B2, Junior + Mid | ~29 | Ürün "dolu" hissettirir, tek kişi üretebilir |
| **Faz 2** | B3 + B4, Junior + Mid | +24 | Toplam ~53: launch için yeterli ağ |
| **Faz 3** | B1-B4 Senior + B5-B8 Jr/Mid | +45 | Retention seviyesi |
| **Faz 4** | Kalan Senior + tüm Expert | +56 | Tam ağ: 154 |

**Kural:** Haritada tüm hatlar ve duraklar 1. günden görünür; içeriği olmayanlar "yakında + 🔔" durağıdır. Boş harita yerine dolu görünen ama kademeli açılan bir ağ — onboarding'deki talep-sinyali mantığının durak düzeyine inmiş hâli. Hangi durağın önce yazılacağına zil sayıları karar verebilir.

## İçerik üretim temposu (gerçekçilik kontrolü)

Blok iskeletiyle bir konu ~2-4 saat üretim (Claude taslak + senin teknik onayın). Haftada 3 konu tempoyla Faz 1 ≈ 10 hafta, Faz 2 sonu ≈ 4,5 ay. Instagram "Neden Var?" serisi her yeni durağın hook'undan türetilirse içerik takvimi de kendiliğinden dolar.
