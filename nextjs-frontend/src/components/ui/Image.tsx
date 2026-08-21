'use client';

import Image from 'next/image';
import { ImageProps } from 'next/image';

export default function NextImage(props: ImageProps) {
  return <Image {...props} />;
}
