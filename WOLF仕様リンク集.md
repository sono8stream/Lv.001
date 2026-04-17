# WOLF仕様リンク集

Web 版 / Unity 版の実装や、WOLF RPG エディタのイベント・バイナリ仕様を追うときの参照先メモ。

## 公式ドキュメント

| 種別 | リンク | 用途 |
| --- | --- | --- |
| 公式ヘルプ | https://silversecond.com/WolfRPGEditor/Help/04eventmain.html | イベント作成の基本。イベント編集まわりの入口。 |
| 公式コモンイベント集 | https://silversecond.com/WolfRPGEditor/CommonList/html/ | 基本システム付属コモンイベントの参照。名前ベースで追うときに便利。 |
| 公式サイト | https://silversecond.com/WolfRPGEditor/ | マニュアルや配布物の起点。 |

## 非公式の解析・周辺資料

| 種別 | リンク | 用途 / 注意 |
| --- | --- | --- |
| ウディタバイナリデータ解析情報 | https://kameske027.cloudfree.jp/analysis_woditor.php | 非公式の総合解析ページ。`CommonEvent.dat` / DB / 外部コモンなどの調査メモへの入口。バージョン差異に注意。 |
| イベントコマンド対応表 | https://kameske027.cloudfree.jp/woditor_analysis/pages/eventCodeCorrespondence.html | コマンド番号とイベントコードの対応表。`123=キー入力` `172=イベント処理中断` `180=ウェイト` `212/213=ラベル` `420=上記以外` の確認に便利。 |
| イベントコマンド構造 | https://kameske027.cloudfree.jp/woditor_analysis/pages/eventCommand.html | 各イベントコマンドの引数構造の詳細。`キー入力` のビット仕様や待機フラグ確認用。 |
| コマンドコード仕様 | https://kameske027.cloudfree.jp/woditor_analysis/pages/eventCodeSpecification.html | 仕様外コードや空白行コードの扱い、イベントコードの受理条件を確認するときに使う。 |
| マップファイル解析メモ | https://rgbacrt.seesaa.net/article/437462362.html | `.mps` の簡易バイナリ解析。レイヤーやマップサイズを追うときの参考。バージョン差異に注意。 |
| ウディタ関連ツールまとめ | https://piposozai.blog.fc2.com/blog-entry-551.html | `dat` / `mps` 解析や編集に使われる周辺ツールの入口。 |
| WOLF RPG Editor map parser | https://github.com/G1org1owo/wolfrpg-map-parser | 非公式パーサ実装。実コードベースの参照先。 |
| オープンソースゲームの中身を見る方法 | https://ameblo.jp/yumusurs/entry-12547466963.html | Data フォルダをそのまま公式エディタで開く手順。暗号化なし前提。 |

## このリポジトリ内の参照先

| パス | 用途 |
| --- | --- |
| `文字列最適化調査.md` | 文字列変数まわりの調査結果。対象イベント、共通イベント、期待表示値のメモ。 |
| `web/src/wolf/data.ts` | Web 版のバイナリ読込・コマンド変換ロジック。 |
| `web/src/wolf/runtime.ts` | Web 版のイベント実行・文字列補間・デバッグ表示ロジック。 |
| `Assets/Scripts/Expression/Map/MapEvent/CommandFactory/WolfOperateDbCommandFactory.cs` | Unity 版の DB 操作コマンド解釈。現状は int 系に寄っている。 |
| `Assets/Scripts/Expression/Event/CommonEventStringAccessor.cs` | Unity 版の common event 文字列 accessor。`Set()` 未実装。 |

## 現時点の解釈メモ

- Unity 版実装では、DB 操作コマンドは `ChangeVariableIntCommand` 経由で処理されており、文字列専用の演算仕様は未実装に見える。
- 本家 WOLF RPG エディタ仕様としては、文字列は **代入** と **連結** を前提に考えるのが自然で、減算・除算・剰余などは通常用途では未定義または非対応の可能性が高い。
- 非公式の解析記事はバージョン差異がありうるため、最終的には **実データ / 本家挙動 / この repo の実装** を突き合わせて判断する。
