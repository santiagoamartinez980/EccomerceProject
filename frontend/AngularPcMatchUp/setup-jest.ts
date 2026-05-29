// @ts-ignore
import 'zone.js';
// @ts-ignore
import 'zone.js/testing';

import { getTestBed } from '@angular/core/testing';
import {
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting,
} from '@angular/platform-browser-dynamic/testing';
import { ɵresolveComponentResources } from '@angular/core';
import * as fs from 'fs';
import * as path from 'path';

// Polyfill de fetch para que ɵresolveComponentResources pueda leer archivos .html y .css
function fetchPolyfill(url: string): Promise<{ text: () => Promise<string> }> {
  const filePath = path.resolve(__dirname, url.replace(/^\//, ''));
  let content = '';
  try {
    content = fs.readFileSync(filePath, 'utf-8');
  } catch {
    content = '';
  }
  return Promise.resolve({ text: () => Promise.resolve(content) });
}

beforeAll(async () => {
  await ɵresolveComponentResources(fetchPolyfill as any);
});

getTestBed().initTestEnvironment(
  BrowserDynamicTestingModule,
  platformBrowserDynamicTesting(),
);