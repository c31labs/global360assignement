// Custom Karma config so `npm run test:ci` works on macOS / Linux / Docker
// without callers needing to set CHROME_BIN themselves.
const path = require('path');

const fallbackChrome = '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome';
if (!process.env['CHROME_BIN'] && require('fs').existsSync(fallbackChrome)) {
  process.env['CHROME_BIN'] = fallbackChrome;
}

module.exports = function (config) {
  config.set({
    basePath: '',
    frameworks: ['jasmine', '@angular-devkit/build-angular'],
    plugins: [
      require('karma-jasmine'),
      require('karma-chrome-launcher'),
      require('karma-jasmine-html-reporter'),
      require('karma-coverage'),
      require('@angular-devkit/build-angular/plugins/karma'),
    ],
    customLaunchers: {
      ChromeHeadlessCI: {
        base: 'ChromeHeadless',
        flags: ['--no-sandbox', '--disable-gpu'],
      },
    },
    client: {
      jasmine: {},
      clearContext: false,
    },
    jasmineHtmlReporter: { suppressAll: true },
    coverageReporter: {
      dir: path.join(__dirname, './coverage/web'),
      subdir: '.',
      reporters: [{ type: 'html' }, { type: 'text-summary' }],
    },
    reporters: ['progress', 'kjhtml'],
    browsers: ['Chrome'],
    restartOnFileChange: true,
  });
};
