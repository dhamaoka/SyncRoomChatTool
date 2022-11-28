# SyncRoomChat読み上げちゃん
## なにするもの？
[YAMAHA SyncRoom](https://syncroom.yamaha.com) のチャットを読み上げるツール。

基本、チャット民が**ループバック機能**を使って、部屋の他者に気がついてもらう為のツールではあります。

~~UI要素を監視しますんで、常にSyncRoomの**チャット画面にフォーカスを持っていきます**。~~
~~**チャット画面を閉じる**と、チャット画面の待機に移りますので、Windowsに制御が戻ります。**これは仕様です**。~~
~~但し、他の人がチャットに打ち込むと、チャット画面が起動しますんで、あとは分かるな？~~

v0.0.0.6でフォーカスしなくしました。勘違いでした。セットフォーカスなくてもUI要素取れる様子。

リアルタイム音声合成。基本は、Windowsについてくる HarukaとZiraです。
英文は[VOICEVOX](https://voicevox.hiroshiba.jp/)を入れてても、Ziraで読みます。
VOICEVOXのGithubは[こちら](https://github.com/VOICEVOX)。

VOICEVOXを別途入れておけば、VOICEVOXの音声合成を使います。使わない設定もGUIから出来ます。
スピーチ自体させない設定もメニューにあります。

自分がチャットに気がつけるだけなら、オーディオインターフェイスにループバック機能は要りません。が、SyncRoomの音声モニターをWindowsの音声出力からすることになるでしょう（ようはオーディオインターフェイスでモニターせずに、PC本体から全て音を聞くスタイル）

## 使い方
[ここ](https://github.com/dhamaoka/SyncRoomChatTool/releases/latest)から最新版のZipファイルを落してきて、適当なフォルダに解凍して、あとは実行するだけ。

Zipの中のファイルだけで動くはずですが、.net framework 4.7.2 が必要です。バージョンアップの場合は、.exeファイルだけの配置でいいはずです。

管理者権限へ昇格するようにマニフェストを設定したつもりではありますが、そうじゃなかったら、管理者権限で動かした方がいいです。

## お約束
VOICEVOXを使う場合は、VOICEVOX Engineは先に起動しておいてください。一応自動起動もしますが。

SyncRoomは先に起ち上げておいた方が吉。

## チャットする側で出来る事。
- チャットの行頭に、/c（大小問わず。スペース有無無視）を入れると、読み上げ前のチャイム鳴らすか否かをトグルスイッチします。
  - 初期値はオンです。
  - ただし、オンの状態でも前のコメントとの間隔が5秒以内だと、鳴らしません。
- チャットの行頭に、/s（大小問わず。スペース有無無視）を入れると、読み上げるか否かをトグルスイッチします。
  - 初期値はオンです。

- VOICEVOXをこのツールの使用者が導入している場合
   - チャットの行頭に、/0 のように数値を指定すると、VOICEVOXの音声を変更できます。今の所、0～21まで。
   - 選択出来る音声の詳細は、VOICEVOX の[ウェブサイト](https://voicevox.hiroshiba.jp/)へ。

## 機能
- ランダムな14人から、チャットに入力がある都度、その人にユニークな初期音声を設定します。（VOICEVOX利用の場合）
  - チャットの1行をコロンで区切って、ユーザを認識します。
- GUIあり。SyncRoomのチャットログをこのツールに表示する。
   - フォント変更可能。表示倍率変更可能（老眼な方向け）
- 監視間隔調整可能（イラチな方は早めで。そうでない方は最大10秒）
   - ツイキャスコメント対応のため、下限値を950m秒と一旦したものの、別タスクでウェイトは固定にしたので、500m秒に変更。

## その他

- リンクが貼られた場合の固定音声は、ご自分でお作りください。ライセンス的に私が作ったWavは置くとまずいかなと思いまして。設定すればその声でしゃべります。つかWaveファイルなら何の音声でもいい。
- このツールを使った事でなんぞ不利益や不具合が発生しても、責任取りません。取れません。
