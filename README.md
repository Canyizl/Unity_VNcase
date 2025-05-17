This repo aims to develop a basic system framework for a Visual Novel Game, including Scene and UI design, text reading and typewriter effects, automatic playback and skipping, saving and loading and other functions.

本项目旨在搭建一个基础的视觉小说框架，包含完善的场景设计、UI设计；文本读取与打字机效果；自动翻页与快速翻页、保存与加载等一系列视觉小说功能。

* * *

05/17 version

### 目前完成功能包括：

· IntroCutScene：开场载入logo与视频，包含淡入淡出，支持视频播放一秒后点击跳过。

· MenuScene：UI设置与对应场景跳转，多语言适配（调整后适配至每个场景），菜单Bgm。

· InputScene：在开始游戏前，可让玩家自主输入主人公名称。

· GameScene：人物Img与avatar显示，剧本读取与显示，自适应多选项跳转，人物语音和场景bgm控制，自动播放，快速跳过，进入保存与加载场景，进入历史记录场景，返回菜单场景，退出游戏，选项加载。

· HistoryScene：历史记录实现。

· SaveLoadScene：实现存档与读档的UI设计，支持翻页，支持保存时保存当前场景图片与剧本。

· SettingScene：全屏窗口化设置，分辨率设置，总音量大小设置，背景音大小设置，人声大小设置，恢复默认设置。

· GalleryScene：画廊解锁，支持点击后显示大图，支持翻页。

· CreditsScene：结局字幕展示，播放片尾Bgm，支持点击加速字幕，暂不支持跳过。

### 后续待修复与加入功能包括：

· MenuScene：界面UI美化。

· GameScene：支持大屏独白模式输出剧本。

· SaveLoadScene：加入覆盖存档提醒，删除存档等功能。

· SettingScene：界面UI美化。

