# Değişkenler ve Veri Tipleri

## Summary

Her C# değişkeninin bir tipi vardır ve o tip, her şeyden önce tek bir şeyi belirler: değişken verinin
kendisini mi tutuyor, yoksa ona giden bir referansı mı? İşte bu tek ayrım — Value Type'a karşı
Reference Type — yeni başlayanların karşılaştığı sürprizlerin çoğunu, deneyimli geliştiricileri
yakalayanların da azımsanmayacak bir kısmını açıklar.

## Learning Objectives

- Bir değişkenin gerçekte ne tuttuğunu söylemek.
- Value Type ile Reference Type'ı ayırt etmek ve atamanın her birine ne yaptığını önceden kestirmek.
- Her birinin nerede yaşadığını ve arkasını kimin topladığını açıklamak.
- `null`'ın ne zaman mümkün olduğunu, ne zaman derleyicinin bunu elediğini fark etmek.

## Why This Topic Matters

"Kopyayı değiştirdim, orijinal de değişti" — herkesin yazdığı ilk gerçekten kafa karıştırıcı
hatalardan biridir. Bu, dildeki bir hata değildir. Value Type / Reference Type ayrımıyla, açıklayacak
bir model olmadan, ilk kez karşılaşmaktır.

## Definition

Değişken, bildirilmiş bir tipe ait bir değeri tutan, adlandırılmış bir konumdur. C#'ta tip derleme
zamanında sabitlenir ve değişemez: değişken yeni bir değer alabilir, yeni bir tip asla alamaz.

## Why It Exists

Ayrım vardır, çünkü iki farklı durumda iki farklı şey ucuzdur.

Bir `int` dört bayttır. Kopyalanması neredeyse bedavadır; her birine bir kimlik vermek — bir adres, bir
yaşam süresi, onu izlemek zorunda olan bir toplayıcı — verinin kendisinden çok daha pahalıya patlardı.

On beş alanı ve bir sipariş listesi olan bir `Customer` ise kopyalaması ucuz değildir ve genellikle
kopyalamak da istemezsiniz: programın iki parçasının *aynı müşteriye* bakması beklenir, tesadüfen aynı
şeyi söyleyen iki müşteriye değil.

C# bu yüzden ikisini de sunar. Value Type verinin kendisidir. Reference Type ise veriye ulaşmanın bir
yoludur.

## Problem It Solves

Ayrım olmasaydı her şey için tek bir davranış seçmek zorunda kalırdınız ve iki seçenek de kötüdür.

Her şeyi kopyalarsanız, bir nesneyi metoda geçirmek pahalılaşır ve kimlik kaybolur — "o" siparişi asla
değiştiremezsiniz, yalnızca kendi kopyanızı.

Her şeyi referansla tutarsanız, sıkı bir döngüdeki bir `int`, dört baytlık veri için bir bellek
ayırmanın, bir dolaylılığın ve Garbage Collector'ın ziyaretinin bedelini öder.

## Core Mental Model

İki kutu, ve içlerinde ne olduğu:

```text
Value Type                     Reference Type

  int x = 42                     var a = new Customer()

  ┌────────┐                     ┌────────┐        ┌──────────────┐
  │   42   │                     │  ref ──┼───────▶│  Customer    │
  └────────┘                     └────────┘        │  Name: "Ada" │
   verinin kendisi                bir referans     └──────────────┘
                                                     verinin kendisi

  Stack                          Stack               Heap
  metot bitince gider            metot bitince       artık hiçbir şey erişemeyince
                                 gider               toplanır
```

Atama her zaman **kutunun içindekini** kopyalar. Value Type için bu, verinin kendisidir. Reference Type
için bu, oktur — ve artık iki değişken de aynı nesneyi gösterir.

Hepsi bu kadar. "Orijinal neden değişti" sorularının tamamı o oktur.

## Core Concepts

**Value Type.** `int`, `double`, `bool`, `char`, `DateTime`, her `struct`, her `enum`. Değişken verinin
kendisini tutar. Atama onu kopyalar. Yerel bir değişkense Stack üzerinde yaşar — ve metot dönünce,
Garbage Collector hiç işin içine girmeden yok olur.

**Reference Type.** `class`, `string`, dizi'ler, `List<T>`, `record` (`record struct` olarak
bildirilmediyse). Değişken bir referans tutar. Atama referansı kopyalar. Nesne Heap üzerinde yaşar ve
ona hiçbir şey erişemez hale gelince Garbage Collector tarafından geri kazanılır.

**`null`.** Yalnızca bir Reference Type `null` olabilir — hiçbir şeye giden bir referans. Bir Value Type
her zaman bir değere sahiptir; `int x;` bu yüzden sıfırdır, "boş" değil. Eksik olabilecek bir `int`
istiyorsanız bunu söyleyin: `int?`.

**Tip çıkarımı.** `var`, derleyiciden tipi sizin yerinize yazmasını ister. Yine de statiktir: tip
derleme zamanında sabitlenir ve `dynamic` değildir.

## Basic Example

```csharp
// Value Type: kopya gerçekten kopyadır.
int a = 1;
int b = a;
b = 2;
// a hâlâ 1.

// Reference Type: kopya, aynı nesneye giden ikinci bir oktur.
var first = new List<string> { "x" };
var second = first;
second.Add("y");
// first artık İKİ eleman içeriyor — çünkü baştan beri tek bir liste vardı.
```

İkinci blokta olağandışı hiçbir şey olmadı. `second = first`, tıpkı `int` için yaptığı gibi, kutunun
içindekini kopyaladı. Kutunun içindeki bir referanstı.

## Real-World Scenario

Bir metot `Customer` alıyor, onu "doğruluyor" ve — yardımsever bir biçimde — kontrol etmeden önce
isimdeki boşlukları kırpıyor.

Çağıran taraf, müşterisinin değiştirilmesini hiç istemedi. Ama `Customer` bir Reference Type, metoda
bir referans verildi ve düzenlediği nesne çağıranın nesnesi. İsim artık her yerde kırpılmış durumda —
kırpmadan hiç söz etmeyen kodun içinde.

Gerçek sistemlerde bu hatanın en yaygın biçimidir ve egzotik değildir: bir ok, ve elinde kopya olduğunu
sanan bir metot.

## Best Practices

- `class` mı `struct` mı diye seçmeden *önce*, atamada ne olmasını istediğinizi sorun. Kimlik → class.
  Birbirinin yerine geçebilen veri → struct.
- `struct`'ları küçük ve değişmez tutun. Büyük ve değiştirilebilir bir struct her atamada ve her metot
  çağrısında kopyalanır, ve her kopya bağımsız olarak değiştirilebilir — ki bunu neredeyse hiç kimse
  kastetmez.
- Nullable reference type'ları açın ve uyarıları ciddiye alın. Çalışma zamanındaki bir
  `NullReferenceException`'ı derleme zamanındaki bir hataya çevirirler; bu takas her zaman kârlıdır.
- Metodun adı söylemiyorsa, çağıranın size verdiği nesneyi değiştirmeyin.

## Common Mistakes

**`string`'i Value Type gibi davranıyor sanmak.** O bir Reference Type'tır. Değermiş gibi *hissettirir*,
çünkü değişmezdir — yerinde değiştiremezsiniz, dolayısıyla paylaşılan bir değişikliği kimse görmez. Ok
hâlâ oradadır.

**Değiştirilebilir bir `struct`.** Her kopya bağımsız olarak değişebilir ve kopyalar her yerde
çıkarılır — atama, argüman, `foreach` döngü değişkeni. Değişiklik bir kopyanın üzerine düşer ve
kaybolur; hem de sessizce.

**`null`'ı "boş" sanmak.** `null` bir liste, boş bir liste değildir. Birinin sıfır elemanı vardır;
diğerinin listesi yoktur ve üzerinde dönmeye kalkarsanız hata fırlatır.

**`var`'ın tipi gizlediğini sanmak.** Gizlemez; tip hâlâ sabittir ve hâlâ denetlenir. Yalnızca tipi
*okuyucudan* gizler — anlaşılmayan bir `var` bu yüzden bir okunabilirlik problemidir, doğruluk problemi
değil.

## Trade-Offs

| Value Type | Reference Type |
|---|---|
| Bellek ayırma yok, Garbage Collector üzerinde baskı yok | Heap üzerinde ayrılır; sonra toplanır |
| Her atamada kopyalanır — 4 bayt için ucuz, 400 bayt için değil | Kopyalanan tek bir referanstır, nesne ne kadar büyük olursa olsun |
| Siz istemedikçe `null` olamaz (`int?`) | `null` olabilir — ve en kötü anda olur |
| İki değişken birbirini asla etkileyemez | İki değişken aynı nesne olabilir — ki çoğu zaman amaç budur |

Takas *kimlik* ile *bağımsızlık* arasındadır ve hiçbiri doğru varsayılan değildir. Programın iki parçası
gerçekten aynı şeyi kastediyorsa Reference Type'a uzanın; yalnızca aynı miktarı kastediyorlarsa Value
Type'a.
