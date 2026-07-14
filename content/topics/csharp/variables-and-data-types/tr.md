# Değişkenler ve Veri Tipleri

## Summary

Her C# değişkeni iki şeyden birini tutar: verinin kendisini, ya da veriye ulaşmanın bir yolunu. Hangisi
olduğu, atama yaptığınızda ne olacağını belirler — ve bunu yanlış bilmek, sizi gerçekten şaşırtacak ilk
hatayı doğurur. Bu konu o hatayla başlıyor.

## Learning Objectives

- Bir atamanın ne yapacağını, çalıştırmadan önce kestirmek.
- Herhangi bir tip için, değişkenin gerçekte ne tuttuğunu söylemek.
- Çağırdığınız bir metodun, değiştirmesini hiç istemediğiniz bir nesneyi neden değiştirdiğini açıklamak.
- `class` mı `struct` mı sorusuna, gerekçesini söyleyebileceğiniz bir cevap vermek.

## Why This Topic Matters

"Kopyayı değiştirdim, orijinal de değişti" — yeni başlayan birinin dilden şüphe etmesine yol açan ilk
hatadır. Dilde bir hata yok. Ortada tek bir ok var, ve kimse onu çizmemiş.

## Definition

Değişken, bildirilmiş bir tipe ait bir değeri tutan, adlandırılmış bir konumdur. Tip derleme zamanında
sabitlenir: değişken yeni bir değer alabilir, yeni bir tip asla alamaz.

## Why It Exists

Şunu kafanızda çalıştırın. Ne basar?

```csharp
var first  = new List<string> { "x" };
var second = first;

second.Add("y");

Console.WriteLine(first.Count);
```

Çoğu kişi **1** der. `second` bir kopya, siz kopyaya ekleme yaptınız, `first` hâlâ tek elemanlı olmalı.

**2** basar.

Şimdi aynı şekil, bir `int` ile:

```csharp
int a = 1;
int b = a;

b = 2;

Console.WriteLine(a);      // 1 — tam da beklediğiniz gibi.
```

Aynı sözdizimi. Aynı atama. Zıt sonuç.

Bu bir tutarsızlık değil, ezberlenecek bir şey de değil. Tek bir kuralın iki kez uygulanması — ve o kuralı
görene kadar C# sizi şaşırtmaya devam edecek.

## Problem It Solves

Atama, **değişkenin içindekini** kopyalar.

`int` için değişkenin içindeki şey `1`. Kopyalayın, elinizde birbirinden bağımsız iki tane olur.

`List` için değişkenin içindeki şey **liste değil.** Bir referans — başka bir yerde duran bir listeyi
gösteren bir ok. Oku kopyalayın, elinizde **tek bir listeyi** gösteren iki ok olur.

İlk kod parçasında olağandışı hiçbir şey olmadı. Atama, iki seferde de tam olarak aynı işi yaptı.

Peki C# neden ikisini birden sunuyor? Çünkü tek bir kural, birinde yanlış olurdu.

Her şeyi kopyalarsanız, bir `Customer`'ı metoda geçirmek on beş alanı ve bir sipariş listesini kopyalamak
demektir. Daha kötüsü: "o" müşteriyi asla değiştiremezsiniz, yalnızca kendi kopyanızı.

Her şeyi referansla tutarsanız, sıkı bir döngüdeki bir `int`, dört baytlık veri için bir bellek ayırmanın,
bir dolaylılığın ve `Garbage Collector`'ın ziyaretinin bedelini öder.

## Core Mental Model

İki kutu. İçlerinde ne olduğu, konunun tamamı.

```text
Value Type                        Reference Type

  int x = 42                        var a = new Customer()

  ┌────────┐                        ┌────────┐        ┌──────────────┐
  │   42   │                        │  ref ──┼───────▶│  Customer    │
  └────────┘                        └────────┘        │  Name: "Ada" │
   verinin kendisi                   bir ok           └──────────────┘
                                                        verinin kendisi

  Stack                             Stack               Heap
  metot bitince gider               metot bitince       ona hiçbir şey erişemeyince
                                    gider               toplanır
```

Atama **kutunun içindekini** kopyalar. `Value Type` için bu, verinin kendisi. `Reference Type` için bu, ok.

"Orijinal neden değişti" sorularının tamamı o oktur.

## Core Concepts

**Value Type** — `int`, `double`, `bool`, `DateTime`, her `struct`, her `enum`. Değişken verinin kendisini
tutar. Atama onu kopyalar. Yerel bir değişkense `Stack` üzerinde yaşar ve metot dönünce yok olur —
`Garbage Collector` hiç işin içine girmez.

**Reference Type** — `class`, `string`, dizi, `List<T>`, `record`. Değişken bir referans tutar. Atama
referansı kopyalar. Nesne `Heap` üzerinde yaşar ve ona hiçbir şey erişemez hale gelince geri kazanılır.

**`null`** — yalnızca bir `Reference Type` `null` olabilir: hiçbir şeyi göstermeyen bir ok. Bir `Value Type`
her zaman bir değere sahiptir; `int x;` bu yüzden sıfırdır, "boş" değil. Eksik olabilecek bir `int`
istiyorsanız bunu söyleyin: `int?`.

## Basic Example

```csharp
// Value Type: kopya gerçekten kopyadır.
int a = 1;
int b = a;
b = 2;
// a hâlâ 1.

// Reference Type: kopya, aynı nesneye giden ikinci bir oktur.
var first  = new List<string> { "x" };
var second = first;
second.Add("y");
// first artık İKİ elemanlı — baştan beri tek bir liste vardı.
```

## Real-World Scenario

Bir metot `Customer` alıyor, onu doğruluyor ve — yardımsever bir biçimde — kontrol etmeden önce isimdeki
boşlukları kırpıyor.

Çağıran taraf, müşterisinin değiştirilmesini hiç istemedi. Ama `Customer` bir `Reference Type`, metoda bir
ok verildi, ve düzenlediği nesne çağıranın nesnesi.

İsim artık her yerde kırpılmış durumda — kırpmadan hiç söz etmeyen bir kodun içinde. Gerçek sistemlerde bu
hatanın en yaygın biçimi budur: bir ok, ve elinde kopya olduğunu sanan bir metot.

## Best Practices

- `class` mı `struct` mı diye seçmeden önce, atamada ne olmasını istediğinizi sorun. Kimlik → class.
  Birbirinin yerine geçebilen veri → struct.
- `struct`'ları küçük ve değişmez tutun.
- Nullable reference type'ları açın. Çalışma zamanındaki bir `NullReferenceException`'ı derleme hatasına
  çevirirler; bu takas her zaman kârlıdır.
- Metodun adı söylemiyorsa, çağıranın size verdiği nesneyi değiştirmeyin.

## Common Mistakes

**`string`'i `Value Type` sanmak.** O bir `Reference Type`. Değermiş gibi *hissettirir*, çünkü değişmezdir —
yerinde değiştiremezsiniz, dolayısıyla paylaşılan bir değişikliği kimse görmez. Ok hâlâ oradadır.

**Değiştirilebilir bir `struct`.** Kopyalar her yerde çıkarılır: atama, argüman, `foreach` değişkeni.
Değişiklik bir kopyanın üzerine düşer ve kaybolur — sessizce.

**`null`'ı "boş" sanmak.** `null` bir liste, boş bir liste değildir. Birinin sıfır elemanı vardır;
diğerinin listesi yoktur ve üzerinde dönmeye kalkarsanız hata fırlatır.

## Trade-Offs

| Value Type | Reference Type |
|---|---|
| Bellek ayırma yok, GC baskısı yok | Heap üzerinde ayrılır |
| Her atamada kopyalanır | Tek bir referans kopyalanır |
| Siz istemedikçe `null` olamaz | `null` olabilir — en kötü anda |
| İki değişken birbirini etkilemez | İki değişken tek nesne olabilir |

Seçim **kimlik** ile **bağımsızlık** arasında. Programın iki parçası gerçekten aynı şeyi kastediyorsa
`Reference Type`'a uzanın; yalnızca aynı miktarı kastediyorlarsa `Value Type`'a.

:::note
`string`, değer gibi davranan bir `Reference Type`. Bu işi yapan şey değişmezlik — dilde özel bir istisna
değil.
:::
