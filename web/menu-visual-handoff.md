# Menu visual fix notes

## Fixed

- Menu picture text now respects WOLF `\f[...]` sizing instead of falling back to browser default text sizing.
- Menu cursor pictures now support the scale-based `pictureEffect` used by the original events.
- Menu image replacements can preserve helper width/height, which keeps stretched cursors and helper frames aligned.
- Menu command labels now come from the configured user DB command list instead of degrading into blank entries and `？？？？`.

## Root causes

1. Text-picture rendering stripped WOLF font directives without applying their size.
2. `pictureEffect` type `80` was unimplemented, so cursor scaling behavior was missing.
3. Menu command calculation common event `89` produced placeholder output in the web runtime; the runtime now normalizes the final command list from the project's configured user DB menu settings.

## Regression coverage

Added / updated tests in `tests/string-runtime.test.mjs` for:

- WOLF-sized menu text rendering
- helper-size preservation for picture replacements
- cursor stretch and movement across menu selections
- configured menu command labels replacing placeholders

## Validation commands

```bash
cd web
npm run build
npm run test:debug
```
