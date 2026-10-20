const fs = require('fs');
const path = require('path');
const { chromium } = require('C:/Users/xinzi/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');

const root = path.resolve(__dirname, '..');
const stem = 'char_4235_thumpy';
const output = path.join(root, 'output', 'Idle');

(async () => {
  const browser = await chromium.launch({
    executablePath: 'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
    headless: true,
    args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'],
  });
  try {
    const page = await browser.newPage();
    await page.setContent('<!DOCTYPE html><canvas id="render"></canvas>');
    await page.addScriptTag({ path: path.join(__dirname, 'spine-webgl.js') });
    const info = await page.evaluate(async ({ atlasText, binary, texture }) => {
      const canvas = document.getElementById('render');
      const context = new spine.webgl.ManagedWebGLRenderingContext(canvas, {
        alpha: true, premultipliedAlpha: true, preserveDrawingBuffer: true, antialias: true,
      });
      const img = new Image();
      img.src = texture;
      await img.decode();
      const atlas = new spine.TextureAtlas(atlasText, () => new spine.webgl.GLTexture(context, img));
      const data = new spine.SkeletonBinary(new spine.AtlasAttachmentLoader(atlas))
        .readSkeletonData(Uint8Array.from(atob(binary), c => c.charCodeAt(0)));
      const animation = data.findAnimation('Idle');
      if (!animation) throw new Error('Idle animation missing');
      const skeleton = new spine.Skeleton(data);
      const fps = 20;
      const count = Math.round(animation.duration * fps);
      function pose(t) {
        skeleton.setToSetupPose();
        animation.apply(skeleton, -1, t, true, [], 1, spine.MixBlend.replace, spine.MixDirection.mixIn);
        skeleton.updateWorldTransform();
      }
      let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
      const offset = new spine.Vector2(), size = new spine.Vector2();
      for (let i = 0; i < count; i++) {
        pose(i / fps);
        skeleton.getBounds(offset, size, []);
        minX = Math.min(minX, offset.x); minY = Math.min(minY, offset.y);
        maxX = Math.max(maxX, offset.x + size.x); maxY = Math.max(maxY, offset.y + size.y);
      }
      const padding = 8;
      const left = Math.floor(minX) - padding, bottom = Math.floor(minY) - padding;
      const width = Math.ceil(maxX) + padding - left, height = Math.ceil(maxY) + padding - bottom;
      canvas.width = width; canvas.height = height;
      const renderer = new spine.webgl.SceneRenderer(canvas, context, true);
      renderer.camera.position.set(left + width / 2, bottom + height / 2, 0);
      renderer.camera.setViewport(width, height);
      const gl = context.gl;
      // Accumulate coverage correctly when rendering onto a transparent target.
      gl.blendFunc = (src, dst) => gl.blendFuncSeparate(src, dst, gl.ONE, gl.ONE_MINUS_SRC_ALPHA);
      window.renderFrame = index => {
        pose(index / fps);
        gl.viewport(0, 0, width, height);
        gl.clearColor(0, 0, 0, 0);
        gl.clear(gl.COLOR_BUFFER_BIT);
        renderer.begin();
        renderer.drawSkeleton(skeleton, false);
        renderer.end();
        return canvas.toDataURL('image/png').split(',')[1];
      };
      return { animation: animation.name, duration: animation.duration, fps, count, width, height,
        bounds: { left, bottom }, version: data.version };
    }, {
      atlasText: fs.readFileSync(path.join(root, stem + '.atlas'), 'utf8'),
      binary: fs.readFileSync(path.join(root, stem + '.skel')).toString('base64'),
      texture: 'data:image/png;base64,' + fs.readFileSync(path.join(root, stem + '.png')).toString('base64'),
    });
    fs.mkdirSync(output, { recursive: true });
    for (let i = 0; i < info.count; i++) {
      const png = await page.evaluate(i => window.renderFrame(i), i);
      fs.writeFileSync(path.join(output, `${stem}-Idle_${String(i).padStart(2, '0')}.png`), Buffer.from(png, 'base64'));
    }
    fs.writeFileSync(path.join(__dirname, 'export-info.json'), JSON.stringify(info, null, 2));
    console.log(JSON.stringify(info));
  } finally {
    await browser.close();
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
