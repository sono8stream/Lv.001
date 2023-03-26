using System;
using System.Collections.Generic;

namespace Expression.Map.MapEvent.Command
{
    public class WolfIntAccessorFactory : Common.IDataAccessorFactory<int>
    {
        private int rawVal;

        public WolfIntAccessorFactory(int rawVal)
        {
            this.rawVal = rawVal;
        }

        public Common.IDataAccessor<int> Create()
        {
            if (rawVal >= 1300000000)
            {
                // システムDB読み出し
            }
            else if (rawVal >= 1100000000)
            {
                // 可変DB読み出し
            }
            else if (rawVal >= 1000000000)
            {
                // ユーザーDB読み出し
            }
            else if (rawVal >= 15000000)
            {
                // コモンイベントのセルフ変数呼び出し
            }
            else if (rawVal >= 9900000)
            {
                // システムＤＢ[5:システム文字列]呼び出し
            }
            else if (rawVal >= 9190000)
            {
                // 実行したマップイベントの情報を呼び出し
            }
            else if (rawVal >= 9180000)
            {
                // 主人公か仲間の情報を呼び出し
            }
            else if (rawVal >= 9100000)
            {
                // 指定したマップイベントの情報を呼び出し
            }
            else if (rawVal >= 9000000)
            {
                // システムＤＢ[6:システム変数名]呼び出し
            }
            else if (rawVal >= 8000000)
            {
                // 乱数呼び出し
            }
            else if (rawVal >= 3000000)
            {
                // システムＤＢ[4:文字列変数名]呼び出し
            }
            else if (rawVal >= 2000000)
            {
                // システムＤＢ[14:通常変数名]もしくはシステムＤＢ[15:予備変数1]～[23:予備変数9]呼び出し
            }
            else if (rawVal >= 1600000)
            {
                // 実行中のコモンイベントのセルフ変数呼び出し
            }
            else if (rawVal >= 1100000)
            {
                // 実行中のマップイベントのセルフ変数呼び出し
                //var repository = DI.DependencyInjector.It().ExpressionDataRpository;
                //Domain.Data.DataRef dataRef = new Domain.Data.DataRef(
                //    new Domain.Data.TableId(mapId.Value, ""),
                //    new Domain.Data.RecordId(eventId.Value, ""),
                //    new Domain.Data.FieldId(rawVal % 10, "")
                //    );
                //return new Common.RepositoryIntAccessor(repository, dataRef);
            }
            else if (rawVal >= 1000000)
            {
                // 指定したマップイベントのセルフ変数呼び出し
            }

            // 特殊条件以外の場合、定数を取得
            return new Common.ConstDataAccessor<int>(rawVal);
        }
    }
}
