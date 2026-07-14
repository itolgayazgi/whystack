import { useRouter } from 'expo-router';
import { useState } from 'react';
import type { SkillLevel } from '../api/preferences';
import { ChoiceScreen } from '../screens/onboarding/choice-screen';
import { OnboardingRoadmapScreen } from '../screens/onboarding/roadmap-screen';
import { WelcomeScreen } from '../screens/onboarding/welcome-screen';
import { useLanguage } from '../state/language';
import { useOnboarding } from '../state/onboarding';

/**
 * The flow: promise → ecosystem → level → roadmap.
 *
 * The state machine is HERE, in one component, rather than as four routes with the answers passed in the
 * URL. Four routes would mean a reader can deep-link into step three with no ecosystem chosen, and every
 * screen would have to defend against a state the flow cannot actually produce.
 */
type Step = 'welcome' | 'ecosystem' | 'level' | 'roadmap';

/**
 * The ecosystems, and the three that are not written yet.
 *
 * Hard-coded here rather than fetched, deliberately: this screen is the FIRST thing a person sees, and it
 * must draw before any network call resolves — including on a phone with no signal. The API is the truth
 * about which ecosystems have content; this list is the truth about which ones the product intends to have,
 * and those are different questions.
 */
const ECOSYSTEMS = [
  { key: 'dotnet', label: '.NET' },
  { key: 'java', label: 'Java', comingSoon: true },
  { key: 'nodejs', label: 'Node.js', comingSoon: true },
  { key: 'php', label: 'PHP', comingSoon: true },
] as const;

const LEVELS: {
  key: SkillLevel;
  labelKey:
    | 'settings.skill.junior'
    | 'settings.skill.midLevel'
    | 'settings.skill.senior'
    | 'settings.skill.expert';
}[] = [
  { key: 'Junior', labelKey: 'settings.skill.junior' },
  { key: 'MidLevel', labelKey: 'settings.skill.midLevel' },
  { key: 'Senior', labelKey: 'settings.skill.senior' },
  { key: 'Expert', labelKey: 'settings.skill.expert' },
];

export default function OnboardingRoute() {
  const router = useRouter();
  const { t } = useLanguage();
  const { save } = useOnboarding();

  const [step, setStep] = useState<Step>('welcome');
  const [ecosystem, setEcosystem] = useState<string>();
  const [level, setLevel] = useState<SkillLevel>();

  if (step === 'welcome') {
    return <WelcomeScreen onStart={() => setStep('ecosystem')} />;
  }

  if (step === 'ecosystem') {
    return (
      <ChoiceScreen
        step={1}
        totalSteps={3}
        title={t('onboarding.ecosystem.title')}
        hint={t('onboarding.ecosystem.hint')}
        choices={[...ECOSYSTEMS]}
        selected={ecosystem}
        onSelect={setEcosystem}
        onBack={() => setStep('welcome')}
        onContinue={() => setStep('level')}
      />
    );
  }

  if (step === 'level') {
    return (
      <ChoiceScreen
        step={2}
        totalSteps={3}
        title={t('onboarding.level.title')}
        hint={t('onboarding.level.hint')}
        choices={LEVELS.map((option) => ({ key: option.key, label: t(option.labelKey) }))}
        selected={level}
        onSelect={(key) => setLevel(key as SkillLevel)}
        onBack={() => setStep('ecosystem')}
        onContinue={() => setStep('roadmap')}
      />
    );
  }

  const chosen = LEVELS.find((option) => option.key === level);
  const levelLabel = chosen ? t(chosen.labelKey) : '';

  return (
    <OnboardingRoadmapScreen
      level={levelLabel}
      onBack={() => setStep('level')}
      onSignUp={async () => {
        // Saved BEFORE the redirect. A reader who signs up and comes back must not find their choices gone —
        // and a registration that fails halfway must not take the ecosystem they picked with it.
        await save({ ecosystem, level, completed: true });
        router.push('/register');
      }}
      onSkip={async () => {
        await save({ ecosystem, level, completed: true });
        router.replace('/');
      }}
    />
  );
}
