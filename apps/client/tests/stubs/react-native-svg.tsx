import { View } from 'react-native';

/**
 * A stand-in for react-native-svg under vitest.
 *
 * The real package reaches into React Native's own Flow-typed source (codegenNativeComponent and friends),
 * which Metro transpiles on a device and vitest does not. Chasing that through the bundler would buy
 * nothing: this runner's whole premise, stated at the top of vitest.config.ts, is that it covers the shared
 * web-target render and that NATIVE rendering is verified by a human on Expo Go.
 *
 * <b>What this stub means for coverage, plainly:</b> nothing here proves an SVG draws. It proves the blocks
 * AROUND it do — which is what the checkpoint tests are actually about. SvgBoundary, the real error boundary
 * that catches a malformed diagram during render, is NOT exercised by any test that uses this stub; it is
 * verified on a device.
 */
export function SvgXml(props: { xml: string | null }) {
  return <View testID="svg-stub" accessibilityLabel={props.xml ?? ''} />;
}

export default { SvgXml };
