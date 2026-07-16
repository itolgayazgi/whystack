import type { AppLanguage } from './language';

// UI chrome only. Educational content NEVER appears here — it lives in content/ and arrives via the
// API (CLAUDE.md 1.3). If you find yourself adding an explanation of a concept to this file, stop.
//
// Approved technical terms (Middleware, Dependency Injection, Garbage Collector, …) are preserved
// verbatim in every language (08-api-standards.md, "Technical Terminology"). They are not keys here
// and must not be translated when they appear in a label.

export interface Messages {
  'common.retry': string;
  'common.cancel': string;
  'common.loading': string;
  'common.offline': string;

  'error.generic.title': string;
  'error.generic.body': string;
  'error.network.title': string;
  'error.network.body': string;

  'empty.generic.title': string;
  'empty.generic.body': string;

  'nav.today': string;
  'nav.line': string;
  'nav.explore': string;
  'nav.profile': string;

  'home.greeting': string;
  'home.streak': string;
  'home.continue.kicker': string;
  'home.continue.empty.title': string;
  'home.continue.empty.body': string;
  'home.continue.go': string;
  'home.line.mine': string;
  'home.station.here': string;
  'home.station.done': string;
  'home.station.next': string;
  'home.station.ahead': string;
  'home.transfer': string;
  'home.empty.line': string;

  'checkpoint.correct': string;
  'checkpoint.tryAgain.lead': string;
  'checkpoint.tryAgain.button': string;
  'checkpoint.tryAgain.hint': string;

  'explore.title': string;
  'explore.placeholder': string;
  'explore.hint': string;
  'explore.empty': string;
  'explore.failed': string;

  'settings.title': string;

  'settings.theme': string;
  'settings.theme.hint': string;
  'settings.theme.system': string;
  'settings.theme.light': string;
  'settings.theme.dark': string;

  'settings.reading': string;
  'settings.reading.scale': string;
  'settings.reading.scale.hint': string;
  'settings.reading.sample': string;

  'settings.motion': string;
  'settings.motion.hint': string;
  'settings.motion.reduce': string;
  'settings.motion.deviceAlreadyOn': string;

  'settings.skill': string;
  'settings.skill.hint': string;
  'settings.skill.notStated': string;
  'settings.skill.junior': string;
  'settings.skill.midLevel': string;
  'settings.skill.senior': string;
  'settings.skill.expert': string;

  'settings.account': string;

  /** Shown after a save lands. Silence is not confirmation — a user who sees nothing assumes nothing happened. */
  'settings.saved': string;
  'settings.saved.body': string;
  'settings.saveFailed': string;
  /** The other device won. We reloaded; the user decides what to do about it. */
  'settings.conflict.title': string;
  'settings.conflict.body': string;
  'settings.unreachable': string;

  'onboarding.promise': string;
  'onboarding.promise.body': string;
  'onboarding.start': string;
  'onboarding.comingSoon': string;
  'onboarding.ecosystem.title': string;
  'onboarding.ecosystem.hint': string;
  'onboarding.level.title': string;
  'onboarding.level.hint': string;
  'onboarding.roadmap.title': string;
  'onboarding.roadmap.hint': string;
  'onboarding.roadmap.warning.title': string;
  'onboarding.roadmap.warning.body': string;
  'onboarding.roadmap.empty.title': string;
  'onboarding.roadmap.empty.body': string;
  'onboarding.roadmap.keep': string;
  'onboarding.roadmap.later': string;
  'common.back': string;
  'common.continue': string;

  'topic.implementation': string;

  /** Where the back label goes on the FIRST stop of a line — there is no stop before it to name. */
  'topic.backToMyLine': string;

  /** The design's chip: "MID · DURAK 4/12 · MEKANİZMA" (durak-ici-konu-ekrani.html). */
  'topic.stopPosition': string;

  'topic.readingTime': string;
  'topic.lastReviewed': string;
  'topic.draft': string;
  'topic.draft.body': string;
  'topic.deprecated': string;
  'topic.prerequisites': string;
  'topic.related': string;
  'topic.next': string;
  'topic.contents': string;
  'topic.notFound.title': string;
  'topic.notFound.body': string;
  'topic.versions': string;

  'topic.section.Summary': string;
  'topic.section.LearningObjectives': string;
  'topic.section.WhyThisTopicMatters': string;
  'topic.section.Prerequisites': string;
  'topic.section.Definition': string;
  'topic.section.WhyItExists': string;
  'topic.section.ProblemItSolves': string;
  'topic.section.HistoricalContext': string;
  'topic.section.CoreMentalModel': string;
  'topic.section.CoreConcepts': string;
  'topic.section.InternalMechanics': string;
  'topic.section.Syntax': string;
  'topic.section.BasicExample': string;
  'topic.section.ProgressiveExamples': string;
  'topic.section.RealWorldScenario': string;
  'topic.section.ArchitectureContext': string;
  'topic.section.PerformanceConsiderations': string;
  'topic.section.SecurityConsiderations': string;
  'topic.section.TestingConsiderations': string;
  'topic.section.BestPractices': string;
  'topic.section.CommonMistakes': string;
  'topic.section.TradeOffs': string;
  'topic.section.Alternatives': string;
  'topic.section.VersionNotes': string;
  'topic.section.InterviewQuestions': string;
  'topic.section.Quiz': string;
  'topic.section.RelatedTopics': string;
  'topic.section.NextRecommendedTopic': string;
  'topic.section.FurtherReading': string;
  'topic.section.Glossary': string;

  'learn.title': string;
  'learn.empty.title': string;
  'learn.empty.body': string;

  'language.app': string;
  'language.app.hint': string;
  'language.content': string;
  'language.content.hint': string;
  'language.name.en': string;
  'language.name.tr': string;

  /** Shown whenever content came back in a language other than the one asked for. Never suppressed. */
  'language.fallback.notice': string;
  'language.fallback.reason.translation_not_available': string;
  'language.fallback.reason.translation_outdated': string;
  'language.fallback.reason.version_not_available': string;

  'home.title': string;
  'home.tagline': string;

  // ---------------------------------------------------------------------------------------------
  // Authentication
  //
  // `04` requires error states to be LOCALIZED. The API's Problem Details carry an English `title`
  // and `detail` written for a developer reading a log — putting those on screen would drop English
  // into a Turkish interface. So the UI keys every message off the stable `code` (`08` guarantees the
  // codes are stable and that clients may depend on them) and never renders the server's prose.
  // ---------------------------------------------------------------------------------------------

  'auth.email': string;
  'auth.password': string;
  'auth.newPassword': string;
  'auth.displayName': string;
  'auth.displayName.hint': string;
  'auth.password.hint': string;

  'auth.signIn.title': string;
  'auth.signIn.submit': string;
  'auth.signIn.noAccount': string;
  'auth.signIn.forgot': string;

  'auth.register.title': string;
  'auth.register.submit': string;
  'auth.register.haveAccount': string;

  /**
   * Registration answers the same way whether or not the address was taken — that is the
   * account-enumeration defence (`04`), and it is built into the API. This screen is what stops the UI
   * from undoing it: there is exactly one success message, and it says nothing about whether an
   * account was created.
   */
  'auth.register.sent.title': string;
  'auth.register.sent.body': string;

  'auth.forgot.title': string;
  'auth.forgot.body': string;
  'auth.forgot.submit': string;
  'auth.forgot.sent.title': string;
  'auth.forgot.sent.body': string;

  'auth.reset.title': string;
  'auth.reset.body': string;
  'auth.reset.submit': string;
  'auth.reset.done.title': string;
  'auth.reset.done.body': string;
  'auth.reset.done.action': string;

  'auth.confirm.working': string;
  'auth.confirm.done.title': string;
  'auth.confirm.done.body': string;
  'auth.confirm.done.action': string;

  'auth.signOut': string;
  'auth.signedInAs': string;

  /** Shown while the session is being restored at launch — before we know whether there is one. */
  'auth.restoring': string;

  /** The offline state. NOT "signed out": the session may be perfectly valid, we just cannot reach it. */
  'auth.unreachable.title': string;
  'auth.unreachable.body': string;

  // Validation the client performs itself, so the common mistakes never cost a round trip and the
  // message arrives in the user's own language.
  'validation.email.required': string;
  'validation.email.invalid': string;
  'validation.password.required': string;
  'validation.password.tooShort': string;
  'validation.password.tooLong': string;
  'validation.password.containsEmail': string;
  'validation.token.missing': string;

  // Keyed off the API's stable error codes (`08`).
  'error.invalid_credentials': string;
  'error.account_locked': string;
  'error.invalid_refresh_token': string;
  'error.invalid_reset_token': string;
  'error.invalid_confirmation_token': string;
  'error.rate_limit_exceeded': string;
  'error.validation_failed': string;
  'error.concurrency_conflict': string;
  'error.resource_not_found': string;

  /** A 422 whose field messages the client cannot localize. See error-messages.ts. */
  'error.field.invalid': string;
}

export type MessageKey = keyof Messages;

const en: Messages = {
  'common.retry': 'Retry',
  'common.cancel': 'Cancel',
  'common.loading': 'Loading',
  'common.offline': 'Offline',

  'error.generic.title': 'Something went wrong',
  'error.generic.body': 'The action could not be completed. Nothing was changed.',
  'error.network.title': 'No connection',
  'error.network.body': 'You appear to be offline. Downloaded topics are still available.',

  'empty.generic.title': 'Nothing here yet',
  'empty.generic.body': 'There is nothing to show on this screen at the moment.',

  'nav.today': 'Today',
  'nav.line': 'My line',
  'nav.explore': 'Explore',
  'nav.profile': 'Profile',

  'home.greeting': 'Welcome back,',
  'home.streak': '{days} days',
  'home.continue.kicker': 'You are in the middle of',
  'home.continue.empty.title': 'You have not started a topic yet.',
  'home.continue.empty.body': 'We do not impose an order — start at whichever stop you are curious about.',
  'home.continue.go': 'Continue',
  'home.line.mine': '{ecosystem} line',
  'home.station.here': 'YOU ARE HERE',
  'home.station.done': 'Done',
  'home.station.next': 'Next stop',
  'home.station.ahead': 'Further along',
  'home.transfer': 'Transfer: this stop meets “{topic}” on the {domain} line.',
  'home.empty.line': 'No published stops on this line yet.',

  'checkpoint.correct': 'That is it.',
  'checkpoint.tryAgain.lead': 'Nice try — I am sure you will find it next time.',
  'checkpoint.tryAgain.button': 'Answer again',
  'checkpoint.tryAgain.hint': 'I want to be sure you are ready for the next topic.',

  'explore.title': 'Explore',
  'explore.placeholder': 'Search a topic or concept…',
  'explore.hint': 'Type at least three letters.',
  'explore.empty': 'Nothing for “{term}”. An unreviewed draft does not show up in search either.',
  'explore.failed': 'The search could not run. Check your connection.',

  'settings.title': 'Settings',

  'settings.theme': 'Theme',
  'settings.theme.hint': 'System follows your device, and is what most people want.',
  'settings.theme.system': 'System',
  'settings.theme.light': 'Light',
  'settings.theme.dark': 'Dark',

  'settings.reading': 'Reading',
  'settings.reading.scale': 'Text size',
  'settings.reading.scale.hint':
    'Changes the size of topics, not of the interface. Your device’s own text-size setting still applies.',
  'settings.reading.sample':
    'A garbage collector exists because manual memory management is a source of bugs that people, reliably, get wrong.',

  'settings.motion': 'Motion',
  'settings.motion.hint': 'Turns off animation and transitions.',
  'settings.motion.reduce': 'Reduce motion',
  'settings.motion.deviceAlreadyOn':
    'Your device already asks for reduced motion, so this is on regardless. Turning it off here will not override your device.',

  'settings.skill': 'Experience',
  'settings.skill.hint': 'Used to pick a starting depth. You can always read anything.',
  'settings.skill.notStated': 'Not stated',
  'settings.skill.junior': 'Junior',
  'settings.skill.midLevel': 'Mid-level',
  'settings.skill.senior': 'Senior',
  'settings.skill.expert': 'Expert',

  'settings.account': 'Account',

  'settings.saved': 'Saved',
  'settings.saved.body': 'Saved to your account. Your other devices will pick this up.',
  'settings.saveFailed': 'That could not be saved. Nothing was changed.',
  'settings.conflict.title': 'Changed on another device',
  'settings.conflict.body':
    'These settings were changed somewhere else while this screen was open. What you see now is what is actually saved. Apply your change again if you still want it.',
  'settings.unreachable': 'Cannot reach the server. Your settings were not saved.',

  'onboarding.promise': 'Why before how.',
  'onboarding.promise.body': 'Learn technologies by the reasons they exist.',
  'onboarding.start': 'Start',
  'onboarding.comingSoon': 'SOON',
  'onboarding.ecosystem.title': 'Which ecosystem?',
  'onboarding.ecosystem.hint':
    'Backend implementations come from this ecosystem. The reasoning does not — it is the same everywhere.',
  'onboarding.level.title': 'Where are you now?',
  'onboarding.level.hint': 'This changes the depth, never the truth. You can move it later.',
  'onboarding.roadmap.title': 'Your roadmap',
  'onboarding.roadmap.hint': 'Where a {level} engineer starts, and what each step is for.',
  'onboarding.roadmap.warning.title': 'This roadmap lives on this phone',
  'onboarding.roadmap.warning.body':
    'Reinstall the app, or open it on a laptop, and it is gone. Create an account and it follows you — along with what you have read and where you stopped.',
  'onboarding.roadmap.empty.title': 'Nothing published yet',
  'onboarding.roadmap.empty.body':
    'Topics are written and reviewed before they are published. Yours will appear here as they land — a made-up roadmap would be the first thing this product lied to you about.',
  'onboarding.roadmap.keep': 'Create an account and keep it',
  'onboarding.roadmap.later': 'Later',
  'common.back': 'Back',
  'common.continue': 'Continue',

  'topic.implementation': 'Implementation',
  'topic.backToMyLine': '← My line',
  'topic.stopPosition': 'Stop {position}/{total}',
  'topic.readingTime': '{minutes} min read',
  'topic.lastReviewed': 'Last reviewed {date}',
  'topic.draft': 'Draft — not published',
  'topic.draft.body': 'You can see this because you review content. Nobody else can.',
  'topic.deprecated': 'Deprecated',
  'topic.prerequisites': 'Read these first',
  'topic.related': 'Related topics',
  'topic.next': 'Next topic',
  'topic.contents': 'On this page',
  'topic.notFound.title': 'No such topic',
  'topic.notFound.body': 'It may have been renamed, or it may not be published yet.',
  'topic.versions': 'Applies to {versions}',

  'topic.section.Summary': 'Summary',
  'topic.section.LearningObjectives': 'Learning objectives',
  'topic.section.WhyThisTopicMatters': 'Why this topic matters',
  'topic.section.Prerequisites': 'Prerequisites',
  'topic.section.Definition': 'Definition',
  'topic.section.WhyItExists': 'Why it exists',
  'topic.section.ProblemItSolves': 'The problem it solves',
  'topic.section.HistoricalContext': 'Historical context',
  'topic.section.CoreMentalModel': 'Core mental model',
  'topic.section.CoreConcepts': 'Core concepts',
  'topic.section.InternalMechanics': 'Internal mechanics',
  'topic.section.Syntax': 'Syntax',
  'topic.section.BasicExample': 'Basic example',
  'topic.section.ProgressiveExamples': 'Progressive examples',
  'topic.section.RealWorldScenario': 'Real-world scenario',
  'topic.section.ArchitectureContext': 'Architecture context',
  'topic.section.PerformanceConsiderations': 'Performance',
  'topic.section.SecurityConsiderations': 'Security',
  'topic.section.TestingConsiderations': 'Testing',
  'topic.section.BestPractices': 'Best practices',
  'topic.section.CommonMistakes': 'Common mistakes',
  'topic.section.TradeOffs': 'Trade-offs',
  'topic.section.Alternatives': 'Alternatives',
  'topic.section.VersionNotes': 'Version notes',
  'topic.section.InterviewQuestions': 'Interview questions',
  'topic.section.Quiz': 'Quiz',
  'topic.section.RelatedTopics': 'Related topics',
  'topic.section.NextRecommendedTopic': 'Next topic',
  'topic.section.FurtherReading': 'Further reading',
  'topic.section.Glossary': 'Glossary',

  'learn.title': 'Learn',
  'learn.empty.title': 'No topics yet',
  'learn.empty.body':
    'Topics are written, reviewed and published from the content repository. The first ones land in Sprint 3. Nothing is broken — there is simply nothing here yet.',

  'language.app': 'Interface language',
  'language.app.hint': 'Labels, buttons and messages.',
  'language.content': 'Content language',
  'language.content.hint':
    'Topics themselves. Independent of the interface — a topic may only exist in English, and you will be told when that happens.',
  'language.name.en': 'English',
  'language.name.tr': 'Türkçe',

  'language.fallback.notice': 'Shown in {returned} — not available in {requested}.',
  'language.fallback.reason.translation_not_available': 'This topic has not been translated yet.',
  'language.fallback.reason.translation_outdated': 'The translation is behind the original and was not used.',
  'language.fallback.reason.version_not_available': 'This version does not exist in the requested language.',

  'home.title': 'WhyStack',
  'home.tagline': 'Learn why technologies exist — not just how to use them.',

  'auth.email': 'Email',
  'auth.password': 'Password',
  'auth.newPassword': 'New password',
  'auth.displayName': 'Display name',
  'auth.displayName.hint': 'Optional. What other people see. You can change it later.',
  'auth.password.hint': 'At least {min} characters. Length is what matters — a passphrase beats a symbol.',

  'auth.signIn.title': 'Sign in',
  'auth.signIn.submit': 'Sign in',
  'auth.signIn.noAccount': 'Create an account',
  'auth.signIn.forgot': 'Forgotten your password?',

  'auth.register.title': 'Create an account',
  'auth.register.submit': 'Create account',
  'auth.register.haveAccount': 'I already have an account',

  'auth.register.sent.title': 'Check your email',
  'auth.register.sent.body':
    'If that address can be registered, we have sent it a confirmation email. Open it to finish setting up your account.',

  'auth.forgot.title': 'Reset your password',
  'auth.forgot.body': 'Enter your email address and we will send you a link to set a new password.',
  'auth.forgot.submit': 'Send the link',
  'auth.forgot.sent.title': 'Check your email',
  'auth.forgot.sent.body':
    'If that address has an account, we have sent it a link to reset the password. The link works once and expires shortly.',

  'auth.reset.title': 'Set a new password',
  'auth.reset.body': 'This signs you out everywhere else. Any other device will have to sign in again.',
  'auth.reset.submit': 'Set the password',
  'auth.reset.done.title': 'Password changed',
  'auth.reset.done.body': 'Your password has been changed and every other session has been ended.',
  'auth.reset.done.action': 'Sign in',

  'auth.confirm.working': 'Confirming your account',
  'auth.confirm.done.title': 'Account confirmed',
  'auth.confirm.done.body': 'Your email address is confirmed. You can sign in now.',
  'auth.confirm.done.action': 'Sign in',

  'auth.signOut': 'Sign out',
  'auth.signedInAs': 'Signed in as {email}',

  'auth.restoring': 'Restoring your session',

  'auth.unreachable.title': 'Cannot reach WhyStack',
  'auth.unreachable.body':
    'You have not been signed out — we simply cannot tell right now. Check your connection and try again.',

  'validation.email.required': 'An email address is required.',
  'validation.email.invalid': 'That does not look like an email address.',
  'validation.password.required': 'A password is required.',
  'validation.password.tooShort': 'Use at least {min} characters.',
  'validation.password.tooLong': 'Use at most {max} characters.',
  'validation.password.containsEmail': 'Your password must not contain your email address.',
  'validation.token.missing': 'This link is incomplete. Open the link from your email again.',

  'error.invalid_credentials': 'That email address and password do not match an account.',
  'error.account_locked':
    'This account is locked after too many failed attempts. Try again in a few minutes, or reset your password.',
  'error.invalid_refresh_token': 'Your session has ended. Please sign in again.',
  'error.invalid_reset_token':
    'This reset link no longer works — it has been used, or it has expired. Request a new one.',
  'error.invalid_confirmation_token':
    'This confirmation link no longer works — it has been used, or it has expired. Request a new one.',
  'error.rate_limit_exceeded': 'Too many attempts. Wait a minute and try again.',
  'error.validation_failed': 'Check the highlighted fields.',
  'error.concurrency_conflict':
    'These settings were changed on another device. Reload them and apply your change again.',
  'error.resource_not_found': 'That could not be found.',

  'error.field.invalid': 'This value was not accepted.',
};

const tr: Messages = {
  'common.retry': 'Tekrar dene',
  'common.cancel': 'Vazgeç',
  'common.loading': 'Yükleniyor',
  'common.offline': 'Çevrimdışı',

  'error.generic.title': 'Bir şeyler ters gitti',
  'error.generic.body': 'İşlem tamamlanamadı. Hiçbir şey değişmedi.',
  'error.network.title': 'Bağlantı yok',
  'error.network.body': 'Çevrimdışı görünüyorsun. İndirilmiş konular hâlâ açılabilir.',

  'empty.generic.title': 'Burada henüz bir şey yok',
  'empty.generic.body': 'Bu ekranda şu an gösterilecek bir şey bulunmuyor.',

  'nav.today': 'Bugün',
  'nav.line': 'Hattım',
  'nav.explore': 'Keşfet',
  'nav.profile': 'Profil',

  'home.greeting': 'Tekrar hoş geldin,',
  'home.streak': '{days} gün',
  'home.continue.kicker': 'Devam ediyorsun',
  'home.continue.empty.title': 'Henüz bir konuya başlamadın.',
  'home.continue.empty.body': 'Sıra dayatmıyoruz — merak ettiğin duraktan başla.',
  'home.continue.go': 'Devam et',
  'home.line.mine': '{ecosystem} Hattın',
  'home.station.here': 'BURADASIN',
  'home.station.done': 'Bitti',
  'home.station.next': 'Sıradaki durak',
  'home.station.ahead': 'İleride',
  'home.transfer': 'Aktarma: Bu durak, {domain} hattındaki “{topic}” ile kesişiyor.',
  'home.empty.line': 'Bu hatta henüz yayınlanmış durak yok.',

  'checkpoint.correct': 'Doğru.',
  'checkpoint.tryAgain.lead': 'Güzel denemeydi, bir sonraki sefere doğru cevabını bulacağından eminim.',
  'checkpoint.tryAgain.button': 'Tekrar cevapla',
  'checkpoint.tryAgain.hint': 'Bir sonraki konuya hazır olduğundan emin olmalıyım.',

  'explore.title': 'Keşfet',
  'explore.placeholder': 'Konu veya kavram ara…',
  'explore.hint': 'En az üç harf yaz.',
  'explore.empty': '“{term}” için bir şey yok. İncelemeden geçmemiş bir taslak aramada da çıkmaz.',
  'explore.failed': 'Arama yapılamadı. Bağlantını kontrol et.',

  'settings.title': 'Ayarlar',

  'settings.theme': 'Tema',
  'settings.theme.hint': 'Sistem, cihazını izler — çoğu kişinin istediği de budur.',
  'settings.theme.system': 'Sistem',
  'settings.theme.light': 'Açık',
  'settings.theme.dark': 'Koyu',

  'settings.reading': 'Okuma',
  'settings.reading.scale': 'Yazı boyutu',
  'settings.reading.scale.hint':
    'Konuların boyutunu değiştirir, arayüzün değil. Cihazının kendi yazı boyutu ayarı yine geçerli.',
  'settings.reading.sample':
    'Garbage Collector var, çünkü belleği elle yönetmek insanların düzenli olarak yanlış yaptığı bir hata kaynağıdır.',

  'settings.motion': 'Hareket',
  'settings.motion.hint': 'Animasyonları ve geçişleri kapatır.',
  'settings.motion.reduce': 'Hareketi azalt',
  'settings.motion.deviceAlreadyOn':
    'Cihazın zaten azaltılmış hareket istiyor, bu yüzden bu ayar açık kalır. Buradan kapatmak cihazının kararını geçersiz kılmaz.',

  'settings.skill': 'Deneyim',
  'settings.skill.hint': 'Başlangıç derinliğini seçmek için kullanılır. Her şeyi her zaman okuyabilirsin.',
  'settings.skill.notStated': 'Belirtilmedi',
  // NOT translated, and the inconsistency here was the tell: Junior and Senior were left alone while
  // MidLevel became "Orta seviye" and Expert became "Uzman". Half a vocabulary, translated by feel.
  //
  // `08` — Technical Terminology: "The term itself should remain unchanged unless the terminology
  // dictionary explicitly defines an approved alias." These four are the industry's own words for
  // seniority, and Turkish developers use them in English — a job advert in Istanbul says "Senior
  // Backend Developer", not "Kıdemli Arka Uç Geliştirici". Translating them would make the product
  // sound like a machine translation of a job board, and would leave "Junior" sitting next to
  // "Orta seviye" as though they came from different languages. They do not; they come from the same
  // one, and it is not Turkish.
  'settings.skill.junior': 'Junior',
  'settings.skill.midLevel': 'Mid-level',
  'settings.skill.senior': 'Senior',
  'settings.skill.expert': 'Expert',

  'settings.account': 'Hesap',

  'settings.saved': 'Kaydedildi',
  'settings.saved.body': 'Hesabına kaydedildi. Diğer cihazların bunu alacak.',
  'settings.saveFailed': 'Bu kaydedilemedi. Hiçbir şey değişmedi.',
  'settings.conflict.title': 'Başka bir cihazda değiştirildi',
  'settings.conflict.body':
    'Bu ekran açıkken ayarlar başka bir yerde değiştirildi. Şu an gördüğün, gerçekten kayıtlı olan. Değişikliğini hâlâ istiyorsan tekrar uygula.',
  'settings.unreachable': 'Sunucuya ulaşılamıyor. Ayarların kaydedilmedi.',

  'onboarding.promise': "Nasıl'dan önce, neden.",
  'onboarding.promise.body': 'Teknolojileri var olma sebepleriyle öğren.',
  'onboarding.start': 'Başla',
  'onboarding.comingSoon': 'YAKINDA',
  'onboarding.ecosystem.title': 'Ekosistemin hangisi?',
  'onboarding.ecosystem.hint':
    'Backend implementasyonları bu ekosistemden gelir. Gerekçe gelmez — o her yerde aynıdır.',
  'onboarding.level.title': 'Şu an neredesin?',
  'onboarding.level.hint':
    'Bu, anlatımın derinliğini değiştirir; doğruluğunu asla. Sonra da değiştirebilirsin.',
  'onboarding.roadmap.title': 'Yol haritan',
  'onboarding.roadmap.hint': 'Bir {level} mühendisin nereden başladığı, ve her adımın ne işe yaradığı.',
  'onboarding.roadmap.warning.title': 'Bu yol haritası bu telefonda duruyor',
  'onboarding.roadmap.warning.body':
    'Uygulamayı yeniden kurarsan ya da dizüstünde açarsan kaybolur. Hesap oluşturursan seninle gelir — okuduklarınla ve nerede kaldığınla birlikte.',
  'onboarding.roadmap.empty.title': 'Henüz yayınlanmış konu yok',
  'onboarding.roadmap.empty.body':
    'Konular yayınlanmadan önce yazılıp inceleniyor. Yayınlandıkça burada belirecekler — uydurma bir yol haritası, bu ürünün sana söylediği ilk yalan olurdu.',
  'onboarding.roadmap.keep': 'Hesap oluştur ve sakla',
  'onboarding.roadmap.later': 'Sonra',
  'common.back': 'Geri',
  'common.continue': 'Devam',

  'topic.implementation': 'İmplementasyon',
  'topic.backToMyLine': '← Hattım',
  'topic.stopPosition': 'Durak {position}/{total}',
  'topic.readingTime': '{minutes} dk okuma',
  'topic.lastReviewed': 'Son inceleme: {date}',
  'topic.draft': 'Taslak — yayınlanmadı',
  'topic.draft.body': 'Bunu görüyorsun çünkü içeriği sen inceliyorsun. Başka kimse göremez.',
  'topic.deprecated': 'Kullanımdan kaldırıldı',
  'topic.prerequisites': 'Önce bunları oku',
  'topic.related': 'İlgili konular',
  'topic.next': 'Sıradaki konu',
  'topic.contents': 'Bu sayfada',
  'topic.notFound.title': 'Böyle bir konu yok',
  'topic.notFound.body': 'Adı değişmiş olabilir ya da henüz yayınlanmamış olabilir.',
  'topic.versions': '{versions} için geçerli',

  'topic.section.Summary': 'Özet',
  'topic.section.LearningObjectives': 'Öğrenme hedefleri',
  'topic.section.WhyThisTopicMatters': 'Bu konu neden önemli',
  'topic.section.Prerequisites': 'Ön koşullar',
  'topic.section.Definition': 'Tanım',
  'topic.section.WhyItExists': 'Neden var',
  'topic.section.ProblemItSolves': 'Çözdüğü problem',
  'topic.section.HistoricalContext': 'Tarihsel bağlam',
  'topic.section.CoreMentalModel': 'Temel zihinsel model',
  'topic.section.CoreConcepts': 'Temel kavramlar',
  'topic.section.InternalMechanics': 'İç mekanizma',
  'topic.section.Syntax': 'Sözdizimi',
  'topic.section.BasicExample': 'Temel örnek',
  'topic.section.ProgressiveExamples': 'İlerleyen örnekler',
  'topic.section.RealWorldScenario': 'Gerçek dünya senaryosu',
  'topic.section.ArchitectureContext': 'Mimari bağlam',
  'topic.section.PerformanceConsiderations': 'Performans',
  'topic.section.SecurityConsiderations': 'Güvenlik',
  'topic.section.TestingConsiderations': 'Test',
  'topic.section.BestPractices': 'En iyi pratikler',
  'topic.section.CommonMistakes': 'Sık yapılan hatalar',
  'topic.section.TradeOffs': 'Ödünleşimler',
  'topic.section.Alternatives': 'Alternatifler',
  'topic.section.VersionNotes': 'Sürüm notları',
  'topic.section.InterviewQuestions': 'Mülakat soruları',
  'topic.section.Quiz': 'Quiz',
  'topic.section.RelatedTopics': 'İlgili konular',
  'topic.section.NextRecommendedTopic': 'Sıradaki konu',
  'topic.section.FurtherReading': 'İleri okuma',
  'topic.section.Glossary': 'Sözlük',

  'learn.title': 'Öğren',
  'learn.empty.title': 'Henüz konu yok',
  'learn.empty.body':
    'Konular içerik deposunda yazılır, incelenir ve yayımlanır. İlkleri Sprint 3’te gelecek. Bozuk bir şey yok — burada henüz bir şey yok, o kadar.',

  'language.app': 'Arayüz dili',
  'language.app.hint': 'Etiketler, düğmeler ve mesajlar.',
  'language.content': 'İçerik dili',
  'language.content.hint':
    'Konuların kendisi. Arayüzden bağımsızdır — bir konu yalnızca İngilizce olabilir, ve bu olduğunda sana söylenir.',
  'language.name.en': 'English',
  'language.name.tr': 'Türkçe',

  'language.fallback.notice': '{returned} dilinde gösteriliyor — {requested} dilinde mevcut değil.',
  'language.fallback.reason.translation_not_available': 'Bu konu henüz çevrilmedi.',
  'language.fallback.reason.translation_outdated': 'Çeviri aslının gerisinde kaldığı için kullanılmadı.',
  'language.fallback.reason.version_not_available': 'Bu sürüm, istenen dilde bulunmuyor.',

  'home.title': 'WhyStack',
  'home.tagline': 'Teknolojileri nasıl kullanacağını değil, neden var olduklarını öğren.',

  'auth.email': 'E-posta',
  'auth.password': 'Parola',
  'auth.newPassword': 'Yeni parola',
  'auth.displayName': 'Görünen ad',
  'auth.displayName.hint': 'İsteğe bağlı. Başkalarının gördüğü ad. Sonradan değiştirebilirsin.',
  'auth.password.hint':
    'En az {min} karakter. Önemli olan uzunluk — bir parola cümlesi, bir sembolden iyidir.',

  'auth.signIn.title': 'Giriş yap',
  'auth.signIn.submit': 'Giriş yap',
  'auth.signIn.noAccount': 'Hesap oluştur',
  'auth.signIn.forgot': 'Parolanı mı unuttun?',

  'auth.register.title': 'Hesap oluştur',
  'auth.register.submit': 'Hesabı oluştur',
  'auth.register.haveAccount': 'Zaten hesabım var',

  'auth.register.sent.title': 'E-postanı kontrol et',
  'auth.register.sent.body':
    'Bu adres kaydedilebiliyorsa, ona bir doğrulama e-postası gönderdik. Hesabını tamamlamak için e-postayı aç.',

  'auth.forgot.title': 'Parolanı sıfırla',
  'auth.forgot.body': 'E-posta adresini yaz, sana yeni parola belirlemen için bir bağlantı gönderelim.',
  'auth.forgot.submit': 'Bağlantıyı gönder',
  'auth.forgot.sent.title': 'E-postanı kontrol et',
  'auth.forgot.sent.body':
    'Bu adrese ait bir hesap varsa, parolayı sıfırlamak için bir bağlantı gönderdik. Bağlantı bir kez çalışır ve kısa sürede geçersiz olur.',

  'auth.reset.title': 'Yeni parola belirle',
  'auth.reset.body':
    'Bu işlem seni diğer her yerden çıkarır. Başka her cihazın yeniden giriş yapması gerekir.',
  'auth.reset.submit': 'Parolayı belirle',
  'auth.reset.done.title': 'Parola değişti',
  'auth.reset.done.body': 'Parolan değiştirildi ve diğer bütün oturumların sonlandırıldı.',
  'auth.reset.done.action': 'Giriş yap',

  'auth.confirm.working': 'Hesabın doğrulanıyor',
  'auth.confirm.done.title': 'Hesap doğrulandı',
  'auth.confirm.done.body': 'E-posta adresin doğrulandı. Artık giriş yapabilirsin.',
  'auth.confirm.done.action': 'Giriş yap',

  'auth.signOut': 'Çıkış yap',
  'auth.signedInAs': '{email} olarak giriş yapıldı',

  'auth.restoring': 'Oturumun geri yükleniyor',

  'auth.unreachable.title': "WhyStack'e ulaşılamıyor",
  'auth.unreachable.body':
    'Oturumun kapatılmadı — şu an bunu söyleyemiyoruz, o kadar. Bağlantını kontrol edip tekrar dene.',

  'validation.email.required': 'E-posta adresi gerekli.',
  'validation.email.invalid': 'Bu bir e-posta adresine benzemiyor.',
  'validation.password.required': 'Parola gerekli.',
  'validation.password.tooShort': 'En az {min} karakter kullan.',
  'validation.password.tooLong': 'En fazla {max} karakter kullan.',
  'validation.password.containsEmail': 'Parolan e-posta adresini içermemeli.',
  'validation.token.missing': 'Bu bağlantı eksik. E-postandaki bağlantıyı tekrar aç.',

  'error.invalid_credentials': 'Bu e-posta ve parola bir hesapla eşleşmiyor.',
  'error.account_locked':
    'Çok fazla başarısız denemeden sonra bu hesap kilitlendi. Birkaç dakika sonra tekrar dene ya da parolanı sıfırla.',
  'error.invalid_refresh_token': 'Oturumun sona erdi. Lütfen tekrar giriş yap.',
  'error.invalid_reset_token':
    'Bu sıfırlama bağlantısı artık çalışmıyor — kullanılmış ya da süresi dolmuş. Yenisini iste.',
  'error.invalid_confirmation_token':
    'Bu doğrulama bağlantısı artık çalışmıyor — kullanılmış ya da süresi dolmuş. Yenisini iste.',
  'error.rate_limit_exceeded': 'Çok fazla deneme. Bir dakika bekleyip tekrar dene.',
  'error.validation_failed': 'İşaretli alanları kontrol et.',
  'error.concurrency_conflict':
    'Bu ayarlar başka bir cihazda değiştirildi. Yeniden yükleyip değişikliğini tekrar uygula.',
  'error.resource_not_found': 'Bu bulunamadı.',

  'error.field.invalid': 'Bu değer kabul edilmedi.',
};

export const catalogs: Record<AppLanguage, Messages> = { en, tr };

/**
 * Typed lookup. `key` is constrained to MessageKey, so a missing or misspelled key is a compile
 * error rather than a string like "home.titel" rendered to a user. That is the whole reason this is
 * hand-rolled instead of pulling in a runtime i18n library.
 */
export function translate(language: AppLanguage, key: MessageKey, params?: Record<string, string>): string {
  const template = catalogs[language][key];
  if (!params) return template;
  return template.replace(/\{(\w+)\}/g, (match, name: string) => params[name] ?? match);
}
