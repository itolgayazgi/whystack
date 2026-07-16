import type { ReactNode } from 'react';
import { View } from 'react-native';

/**
 * A stand-in for react-native-svg under vitest.
 *
 * The real package reaches into React Native's own Flow-typed source (codegenNativeComponent and friends),
 * which Metro transpiles on a device and vitest does not. Chasing that through the bundler would buy
 * nothing: this runner's whole premise, stated at the top of vitest.config.ts, is that it covers the shared
 * web-target render, and that NATIVE rendering is verified by a human on Expo Go.
 *
 * <b>What this stub means for coverage, plainly:</b> nothing here proves an SVG draws. It proves the things
 * AROUND it do — that the bottom bar renders four tabs, that the checkpoint blocks behave. SvgBoundary, the
 * real error boundary that catches a malformed diagram during render, is NOT exercised by any test that uses
 * this stub; it is verified on a device.
 *
 * <b>Every export the app imports must exist here.</b> A missing one fails at IMPORT, so the test file dies
 * before its first assertion and the error names react-native-svg rather than the component that reached for
 * it — which is a bad ten minutes for whoever reads it next. That is exactly how this file grew: the nav
 * icons imported Path and Circle, and four unrelated navigation tests went red at once.
 */

export function SvgXml(props: { xml: string | null }) {
  return <View testID="svg-stub" accessibilityLabel={props.xml ?? ''} />;
}

export function Svg({ children }: { children?: ReactNode }) {
  return <View testID="svg">{children}</View>;
}

export function Path() {
  return <View testID="svg-path" />;
}

export function Circle() {
  return <View testID="svg-circle" />;
}

export function Rect() {
  return <View testID="svg-rect" />;
}

export function G({ children }: { children?: ReactNode }) {
  return <View>{children}</View>;
}

export default { SvgXml, Svg, Path, Circle, Rect, G };
