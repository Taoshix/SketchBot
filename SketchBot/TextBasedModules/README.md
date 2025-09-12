## Why is there a `TextBasedModules` directory?

The modules (except the DevModule) in the `SketchBot/TextBasedModules` directory are legacy text-based command modules from earlier versions of SketchBot, before Discord slash commands became the standard and `MessageContent` gateway intent became priviledged and enforced.  
Discord has not given Sketch Bot the now prviledged `MessageContent` gateway intent, so there is no point in actively maintaining both module types at once. Commands in the `TextBasedModules` remain accessible via the mention prefix (@sketchbot), but are no longer actively updated or maintained except for critical bug fixes or to resolve compilation errors.

All new development and features are done in the interaction-based modules.  
These legacy modules are kept for compatibility and reference purposes only.