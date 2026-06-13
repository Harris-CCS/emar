
set    @continue_update = 0;
select @continue_update = 1
from [sys].[tables]
where [name] = 'sysdiagrams';

-------------------------------------------------------------------------
-- Summary: Restore diagram [Security] from database [emar].
-------------------------------------------------------------------------
print '=== Restoring diagram [Security] ===';

set nocount on;

set    @version         = 2;
set    @version_current = null;
set    @diagram_id      = null;

if @continue_update = 1
begin

    select
         @diagram_id      = diagram_id
        ,@version_current = version
    from [dbo].[sysdiagrams]
    where [name] = 'Security';

    set @diagram_id      = isnull(@diagram_id,-1)
    set @version_current = isnull(@version_current,-1)

    if @diagram_id <> -1 and @version_current = @version
        begin
            -- skip update if no version changes exist
            set @continue_update = 0;
        end;

end;

if @continue_update = 1
begin
begin try
    if @diagram_id <> -1
        begin
            update [dbo].[sysdiagrams] set
                 [definition]  = 0x
                ,[version]     = @version
            where [diagram_id] = @diagram_id;
        end;
    else
        begin
            insert into [dbo].[sysdiagrams]
                ([name]
               , [principal_id]
               , [version]
               , [definition]
                )
            output [inserted].[diagram_id]
                   into @outputs
            values('Security', 1, @version, 0x);

            select top 1 @diagram_id = [Id]
            from @outputs
            order by [Id] desc;
        end;
end try
begin catch
    print '=== ' + error_message() + ' ===';
    return;
end catch;

begin try
    update dbo.sysdiagrams set definition.write(0xD0CF11E0A1B11AE1000000000000000000000000000000003E000300FEFF0900, null, 0) where diagram_id = @diagram_id; -- index:1
    update dbo.sysdiagrams set definition.write(0x0600000000000000000000000100000001000000000000000010000002000000, null, 0) where diagram_id = @diagram_id; -- index:33
    update dbo.sysdiagrams set definition.write(0x02000000FEFFFFFF0000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:65
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:97
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:129
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:161
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:193
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:225
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:257
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:289
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:321
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:353
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:385
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:417
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:449
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:481
    update dbo.sysdiagrams set definition.write(0xFDFFFFFF0D000000140000000400000005000000060000000700000008000000, null, 0) where diagram_id = @diagram_id; -- index:513
    update dbo.sysdiagrams set definition.write(0x090000000A0000000B0000000C0000000E000000FEFFFFFF0F00000010000000, null, 0) where diagram_id = @diagram_id; -- index:545
    update dbo.sysdiagrams set definition.write(0x11000000120000001300000015000000FEFFFFFFFEFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:577
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:609
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:641
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:673
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:705
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:737
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:769
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:801
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:833
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:865
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:897
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:929
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:961
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:993
    update dbo.sysdiagrams set definition.write(0x52006F006F007400200045006E00740072007900000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1025
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1057
    update dbo.sysdiagrams set definition.write(0x16000500FFFFFFFFFFFFFFFF0200000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1089
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000060CED188B548D60103000000C021000000000000, null, 0) where diagram_id = @diagram_id; -- index:1121
    update dbo.sysdiagrams set definition.write(0x6600000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1153
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1185
    update dbo.sysdiagrams set definition.write(0x04000201FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1217
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000007604000000000000, null, 0) where diagram_id = @diagram_id; -- index:1249
    update dbo.sysdiagrams set definition.write(0x6F00000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1281
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1313
    update dbo.sysdiagrams set definition.write(0x040002010100000004000000FFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1345
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000000000000000012000000840D000000000000, null, 0) where diagram_id = @diagram_id; -- index:1377
    update dbo.sysdiagrams set definition.write(0x010043006F006D0070004F0062006A0000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1409
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1441
    update dbo.sysdiagrams set definition.write(0x12000201FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1473
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000490000005F00000000000000, null, 0) where diagram_id = @diagram_id; -- index:1505
    update dbo.sysdiagrams set definition.write(0x0100000002000000030000000400000005000000060000000700000008000000, null, 0) where diagram_id = @diagram_id; -- index:1537
    update dbo.sysdiagrams set definition.write(0x090000000A0000000B0000000C0000000D0000000E0000000F00000010000000, null, 0) where diagram_id = @diagram_id; -- index:1569
    update dbo.sysdiagrams set definition.write(0x11000000FEFFFFFF130000001400000015000000160000001700000018000000, null, 0) where diagram_id = @diagram_id; -- index:1601
    update dbo.sysdiagrams set definition.write(0x190000001A0000001B0000001C0000001D0000001E0000001F00000020000000, null, 0) where diagram_id = @diagram_id; -- index:1633
    update dbo.sysdiagrams set definition.write(0x2100000022000000230000002400000025000000260000002700000028000000, null, 0) where diagram_id = @diagram_id; -- index:1665
    update dbo.sysdiagrams set definition.write(0x290000002A0000002B0000002C0000002D0000002E0000002F00000030000000, null, 0) where diagram_id = @diagram_id; -- index:1697
    update dbo.sysdiagrams set definition.write(0x3100000032000000330000003400000035000000360000003700000038000000, null, 0) where diagram_id = @diagram_id; -- index:1729
    update dbo.sysdiagrams set definition.write(0x390000003A0000003B0000003C0000003D0000003E0000003F00000040000000, null, 0) where diagram_id = @diagram_id; -- index:1761
    update dbo.sysdiagrams set definition.write(0x4100000042000000430000004400000045000000460000004700000048000000, null, 0) where diagram_id = @diagram_id; -- index:1793
    update dbo.sysdiagrams set definition.write(0xFEFFFFFF4A000000FEFFFFFF4C0000004D0000004E0000004F00000050000000, null, 0) where diagram_id = @diagram_id; -- index:1825
    update dbo.sysdiagrams set definition.write(0x5100000052000000530000005400000055000000560000005700000058000000, null, 0) where diagram_id = @diagram_id; -- index:1857
    update dbo.sysdiagrams set definition.write(0x590000005A0000005B0000005C0000005D0000005E0000005F00000060000000, null, 0) where diagram_id = @diagram_id; -- index:1889
    update dbo.sysdiagrams set definition.write(0x6100000062000000630000006400000065000000660000006700000068000000, null, 0) where diagram_id = @diagram_id; -- index:1921
    update dbo.sysdiagrams set definition.write(0x690000006A0000006B0000006C0000006D0000006E0000006F00000070000000, null, 0) where diagram_id = @diagram_id; -- index:1953
    update dbo.sysdiagrams set definition.write(0x71000000720000007300000074000000750000007600000077000000FEFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:1985
    update dbo.sysdiagrams set definition.write(0xFEFFFFFF7A0000007B0000007C0000007D0000007E0000007F00000080000000, null, 0) where diagram_id = @diagram_id; -- index:2017
    update dbo.sysdiagrams set definition.write(0x000430000A1E100C05000080160000000F00FFFF16000000007D0000339D0000, null, 0) where diagram_id = @diagram_id; -- index:2049
    update dbo.sysdiagrams set definition.write(0xFC5B00003BA400001E670000A0C4FFFF00CEFFFFDE805B10F195D011B0A000AA, null, 0) where diagram_id = @diagram_id; -- index:2081
    update dbo.sysdiagrams set definition.write(0x00BDCB5C000008003000000000020000030000003C006B000000090000000000, null, 0) where diagram_id = @diagram_id; -- index:2113
    update dbo.sysdiagrams set definition.write(0x0000D9E6B0E91C81D011AD5100A0C90F5739F43B7F847F61C74385352986E1D5, null, 0) where diagram_id = @diagram_id; -- index:2145
    update dbo.sysdiagrams set definition.write(0x52F8A0327DB2D86295428D98273C25A2DA2D000028004300000000000000B5B0, null, 0) where diagram_id = @diagram_id; -- index:2177
    update dbo.sysdiagrams set definition.write(0xC832B618F5469CA7016F91DF3A0134C9D2777977D811907000065B840D9C0000, null, 0) where diagram_id = @diagram_id; -- index:2209
    update dbo.sysdiagrams set definition.write(0x280043000000000000007B31EFF5E6FA56429865BD40E34CA05E34C9D2777977, null, 0) where diagram_id = @diagram_id; -- index:2241
    update dbo.sysdiagrams set definition.write(0xD811907000065B840D9C0C00000084030000008C013C00003000A50900000700, null, 0) where diagram_id = @diagram_id; -- index:2273
    update dbo.sysdiagrams set definition.write(0x0080010000009C0200000080000005000080536368477269640014ECFFFFA4D4, null, 0) where diagram_id = @diagram_id; -- index:2305
    update dbo.sysdiagrams set definition.write(0xFFFF736974657369640000003000A509000007000080020000009C0200000080, null, 0) where diagram_id = @diagram_id; -- index:2337
    update dbo.sysdiagrams set definition.write(0x000005000080536368477269640014ECFFFF0CE5FFFF75736572736964000000, null, 0) where diagram_id = @diagram_id; -- index:2369
    update dbo.sysdiagrams set definition.write(0x6400A5090000070000800700000052000000018000003B000080436F6E74726F, null, 0) where diagram_id = @diagram_id; -- index:2401
    update dbo.sysdiagrams set definition.write(0x6C001BF2FFFF0FD9FFFF52656C6174696F6E736869702027666B5F5F75736572, null, 0) where diagram_id = @diagram_id; -- index:2433
    update dbo.sysdiagrams set definition.write(0x735F5F736974657327206265747765656E202773697465732720616E64202775, null, 0) where diagram_id = @diagram_id; -- index:2465
    update dbo.sysdiagrams set definition.write(0x73657273270000002800B5010000070000800800000031000000530000000280, null, 0) where diagram_id = @diagram_id; -- index:2497
    update dbo.sysdiagrams set definition.write(0x0000436F6E74726F6C00E4E9FFFFBDDFFFFF00003800A5090000070000800900, null, 0) where diagram_id = @diagram_id; -- index:2529
    update dbo.sysdiagrams set definition.write(0x0000B2020000008000001000008053636847726964005ECFFFFF80DAFFFF7573, null, 0) where diagram_id = @diagram_id; -- index:2561
    update dbo.sysdiagrams set definition.write(0x65725F7065726D697373696F6E7300003400A5090000070000800A000000A802, null, 0) where diagram_id = @diagram_id; -- index:2593
    update dbo.sysdiagrams set definition.write(0x0000008000000B00008053636847726964005ECFFFFF86F2FFFF7065726D6973, null, 0) where diagram_id = @diagram_id; -- index:2625
    update dbo.sysdiagrams set definition.write(0x73696F6E730000007C00A5090000070000800F00000062000000018000005100, null, 0) where diagram_id = @diagram_id; -- index:2657
    update dbo.sysdiagrams set definition.write(0x0080436F6E74726F6C006BE1FFFF39E1FFFF52656C6174696F6E736869702027, null, 0) where diagram_id = @diagram_id; -- index:2689
    update dbo.sysdiagrams set definition.write(0x666B5F5F757365725F7065726D697373696F6E735F5F75736572732720626574, null, 0) where diagram_id = @diagram_id; -- index:2721
    update dbo.sysdiagrams set definition.write(0x7765656E202775736572732720616E642027757365725F7065726D697373696F, null, 0) where diagram_id = @diagram_id; -- index:2753
    update dbo.sysdiagrams set definition.write(0x6E732700000000002800B5010000070000801000000031000000690000000280, null, 0) where diagram_id = @diagram_id; -- index:2785
    update dbo.sysdiagrams set definition.write(0x0000436F6E74726F6C004BE9FFFFADE0FFFF00008800A5090000070000801100, null, 0) where diagram_id = @diagram_id; -- index:2817
    update dbo.sysdiagrams set definition.write(0x000052000000018000005D000080436F6E74726F6C0027D7FFFF58E6FFFF5265, null, 0) where diagram_id = @diagram_id; -- index:2849
    update dbo.sysdiagrams set definition.write(0x6C6174696F6E736869702027666B5F5F757365725F7065726D697373696F6E73, null, 0) where diagram_id = @diagram_id; -- index:2881
    update dbo.sysdiagrams set definition.write(0x5F5F7065726D697373696F6E7327206265747765656E20277065726D69737369, null, 0) where diagram_id = @diagram_id; -- index:2913
    update dbo.sysdiagrams set definition.write(0x6F6E732720616E642027757365725F7065726D697373696F6E73270000000000, null, 0) where diagram_id = @diagram_id; -- index:2945
    update dbo.sysdiagrams set definition.write(0x2800B50100000700008012000000310000007500000002800000436F6E74726F, null, 0) where diagram_id = @diagram_id; -- index:2977
    update dbo.sysdiagrams set definition.write(0x6C006DD9FFFFDCF0FFFF00007C00A50900000700008015000000520000000180, null, 0) where diagram_id = @diagram_id; -- index:3009
    update dbo.sysdiagrams set definition.write(0x000051000080436F6E74726F6C006BE1FFFF7FD9FFFF52656C6174696F6E7368, null, 0) where diagram_id = @diagram_id; -- index:3041
    update dbo.sysdiagrams set definition.write(0x69702027666B5F5F757365725F7065726D697373696F6E735F5F736974657327, null, 0) where diagram_id = @diagram_id; -- index:3073
    update dbo.sysdiagrams set definition.write(0x206265747765656E202773697465732720616E642027757365725F7065726D69, null, 0) where diagram_id = @diagram_id; -- index:3105
    update dbo.sysdiagrams set definition.write(0x7373696F6E732705000000002800B50100000700008016000000310000006900, null, 0) where diagram_id = @diagram_id; -- index:3137
    update dbo.sysdiagrams set definition.write(0x000002800000436F6E74726F6C009FDFFFFF0FD9FFFF00000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3169
    update dbo.sysdiagrams set definition.write(0x21433412080000004C0F00002207000078563412070000001401000073006900, null, 0) where diagram_id = @diagram_id; -- index:3201
    update dbo.sysdiagrams set definition.write(0x7400650073000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3233
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3265
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3297
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3329
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3361
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3393
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3425
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000020000000500000054000000, null, 0) where diagram_id = @diagram_id; -- index:3457
    update dbo.sysdiagrams set definition.write(0x2C0000002C0000002C00000034000000000000000000000022290000F3100000, null, 0) where diagram_id = @diagram_id; -- index:3489
    update dbo.sysdiagrams set definition.write(0x000000002D010000070000000C000000070000001C0100000609000062070000, null, 0) where diagram_id = @diagram_id; -- index:3521
    update dbo.sysdiagrams set definition.write(0x480300001A040000DF020000EC04000027060000B103000027060000CB070000, null, 0) where diagram_id = @diagram_id; -- index:3553
    update dbo.sysdiagrams set definition.write(0x55050000000000000100000088160000180C0000000000000300000003000000, null, 0) where diagram_id = @diagram_id; -- index:3585
    update dbo.sysdiagrams set definition.write(0x02000000020000001C010000F50A000000000000010000004C0F000022070000, null, 0) where diagram_id = @diagram_id; -- index:3617
    update dbo.sysdiagrams set definition.write(0x00000000010000000100000002000000020000001C010000DB06000001000000, null, 0) where diagram_id = @diagram_id; -- index:3649
    update dbo.sysdiagrams set definition.write(0x0000000039130000340300000000000000000000000000000200000002000000, null, 0) where diagram_id = @diagram_id; -- index:3681
    update dbo.sysdiagrams set definition.write(0x1C010000060900000000000000000000D1310000092300000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3713
    update dbo.sysdiagrams set definition.write(0x0D00000004000000040000001C01000006090000AA0A00009006000078563412, null, 0) where diagram_id = @diagram_id; -- index:3745
    update dbo.sysdiagrams set definition.write(0x040000005400000001000000010000000B000000000000000100000002000000, null, 0) where diagram_id = @diagram_id; -- index:3777
    update dbo.sysdiagrams set definition.write(0x030000000400000005000000060000000700000008000000090000000A000000, null, 0) where diagram_id = @diagram_id; -- index:3809
    update dbo.sysdiagrams set definition.write(0x04000000640062006F0000000600000073006900740065007300000021433412, null, 0) where diagram_id = @diagram_id; -- index:3841
    update dbo.sysdiagrams set definition.write(0x080000004C0F00009D0900007856341207000000140100007500730065007200, null, 0) where diagram_id = @diagram_id; -- index:3873
    update dbo.sysdiagrams set definition.write(0x7300000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3905
    update dbo.sysdiagrams set definition.write(0x000000003616B63900190080CC180360F8160360FFFFFFFF0000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3937
    update dbo.sysdiagrams set definition.write(0x0000000008BD6915000000000000000000000000000088430000404045000020, null, 0) where diagram_id = @diagram_id; -- index:3969
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000038D3BF27000000000000F03F00000000, null, 0) where diagram_id = @diagram_id; -- index:4001
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000000000000000000000000000000000000C7164139, null, 0) where diagram_id = @diagram_id; -- index:4033
    update dbo.sysdiagrams set definition.write(0x001A0080CC180360F8160360FFFFFFFF00000000000000000000000008BD6915, null, 0) where diagram_id = @diagram_id; -- index:4065
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000088430000B041450000200000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4097
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000000000200000005000000540000002C000000, null, 0) where diagram_id = @diagram_id; -- index:4129
    update dbo.sysdiagrams set definition.write(0x2C0000002C000000340000000000000000000000222900003C2C000000000000, null, 0) where diagram_id = @diagram_id; -- index:4161
    update dbo.sysdiagrams set definition.write(0x2D0100000D0000000C000000070000001C010000060900006207000048030000, null, 0) where diagram_id = @diagram_id; -- index:4193
    update dbo.sysdiagrams set definition.write(0x1A040000DF020000EC04000027060000B103000027060000CB07000055050000, null, 0) where diagram_id = @diagram_id; -- index:4225
    update dbo.sysdiagrams set definition.write(0x00000000010000008816000061270000000000000E0000000C00000002000000, null, 0) where diagram_id = @diagram_id; -- index:4257
    update dbo.sysdiagrams set definition.write(0x020000001C010000F50A000000000000010000004C0F00009D09000000000000, null, 0) where diagram_id = @diagram_id; -- index:4289
    update dbo.sysdiagrams set definition.write(0x020000000200000002000000020000001C010000DB0600000100000000000000, null, 0) where diagram_id = @diagram_id; -- index:4321
    update dbo.sysdiagrams set definition.write(0x391300003403000000000000000000000000000002000000020000001C010000, null, 0) where diagram_id = @diagram_id; -- index:4353
    update dbo.sysdiagrams set definition.write(0x060900000000000000000000D13100000923000000000000000000000D000000, null, 0) where diagram_id = @diagram_id; -- index:4385
    update dbo.sysdiagrams set definition.write(0x04000000040000001C01000006090000AA0A0000900600007856341204000000, null, 0) where diagram_id = @diagram_id; -- index:4417
    update dbo.sysdiagrams set definition.write(0x5400000001000000010000000B00000000000000010000000200000003000000, null, 0) where diagram_id = @diagram_id; -- index:4449
    update dbo.sysdiagrams set definition.write(0x0400000005000000060000000700000008000000090000000A00000004000000, null, 0) where diagram_id = @diagram_id; -- index:4481
    update dbo.sysdiagrams set definition.write(0x640062006F0000000600000075007300650072007300000002000B00B2F3FFFF, null, 0) where diagram_id = @diagram_id; -- index:4513
    update dbo.sysdiagrams set definition.write(0xC6DBFFFFB2F3FFFF0CE5FFFF0000000002000000F0F0F0000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4545
    update dbo.sysdiagrams set definition.write(0x0000000000000000010000000800000000000000E4E9FFFFBDDFFFFF1F090000, null, 0) where diagram_id = @diagram_id; -- index:4577
    update dbo.sysdiagrams set definition.write(0x58010000320000000100000200001F0900005801000002000000000005000080, null, 0) where diagram_id = @diagram_id; -- index:4609
    update dbo.sysdiagrams set definition.write(0x0800008001000000150001000000900144420100065461686F6D61100066006B, null, 0) where diagram_id = @diagram_id; -- index:4641
    update dbo.sysdiagrams set definition.write(0x005F005F00750073006500720073005F005F0073006900740065007300214334, null, 0) where diagram_id = @diagram_id; -- index:4673
    update dbo.sysdiagrams set definition.write(0x120800000039130000930E000078563412070000001401000075007300650072, null, 0) where diagram_id = @diagram_id; -- index:4705
    update dbo.sysdiagrams set definition.write(0x005F007000650072006D0069007300730069006F006E00730000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4737
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4769
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4801
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4833
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4865
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4897
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4929
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000000000000000200000005000000540000002C0000, null, 0) where diagram_id = @diagram_id; -- index:4961
    update dbo.sysdiagrams set definition.write(0x002C0000002C00000034000000000000000000000022290000E9150000000000, null, 0) where diagram_id = @diagram_id; -- index:4993
    update dbo.sysdiagrams set definition.write(0x002D010000070000000C000000070000001C0100000609000062070000480300, null, 0) where diagram_id = @diagram_id; -- index:5025
    update dbo.sysdiagrams set definition.write(0x001A040000DF020000EC04000027060000B103000027060000CB070000550500, null, 0) where diagram_id = @diagram_id; -- index:5057
    update dbo.sysdiagrams set definition.write(0x000000000001000000391300000E110000000000000400000004000000020000, null, 0) where diagram_id = @diagram_id; -- index:5089
    update dbo.sysdiagrams set definition.write(0x00020000001C01000015090000000000000100000039130000930E0000000000, null, 0) where diagram_id = @diagram_id; -- index:5121
    update dbo.sysdiagrams set definition.write(0x00040000000400000002000000020000001C0100001509000001000000000000, null, 0) where diagram_id = @diagram_id; -- index:5153
    update dbo.sysdiagrams set definition.write(0x0039130000E502000000000000000000000000000002000000020000001C0100, null, 0) where diagram_id = @diagram_id; -- index:5185
    update dbo.sysdiagrams set definition.write(0x00060900000000000000000000D13100000923000000000000000000000D0000, null, 0) where diagram_id = @diagram_id; -- index:5217
    update dbo.sysdiagrams set definition.write(0x0004000000040000001C01000006090000AA0A00009006000078563412040000, null, 0) where diagram_id = @diagram_id; -- index:5249
    update dbo.sysdiagrams set definition.write(0x006A00000001000000010000000B000000000000000100000002000000030000, null, 0) where diagram_id = @diagram_id; -- index:5281
    update dbo.sysdiagrams set definition.write(0x000400000005000000060000000700000008000000090000000A000000040000, null, 0) where diagram_id = @diagram_id; -- index:5313
    update dbo.sysdiagrams set definition.write(0x00640062006F0000001100000075007300650072005F007000650072006D0069, null, 0) where diagram_id = @diagram_id; -- index:5345
    update dbo.sysdiagrams set definition.write(0x007300730069006F006E00730000002143341208000000391300002207000078, null, 0) where diagram_id = @diagram_id; -- index:5377
    update dbo.sysdiagrams set definition.write(0x56341207000000140100007000650072006D0069007300730069006F006E0073, null, 0) where diagram_id = @diagram_id; -- index:5409
    update dbo.sysdiagrams set definition.write(0x000000B8E79716E1E3D8E0DC3E672C0604A338000B0080D41CFD5C010000C000, null, 0) where diagram_id = @diagram_id; -- index:5441
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000000000000000000000000000000244E7BA235, null, 0) where diagram_id = @diagram_id; -- index:5473
    update dbo.sysdiagrams set definition.write(0xA71D4DB8E79716E1E3D8E02434672C0E049B38000C0080D41CFD5C010000C000, null, 0) where diagram_id = @diagram_id; -- index:5505
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000000000000000000000000000000244E7BA235, null, 0) where diagram_id = @diagram_id; -- index:5537
    update dbo.sysdiagrams set definition.write(0xA71D4DB8E79716E1E3D8E0F44EBB3C16049338040D0080D41CFD5C010000C000, null, 0) where diagram_id = @diagram_id; -- index:5569
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000000000000000000000000181B48244E7BA235, null, 0) where diagram_id = @diagram_id; -- index:5601
    update dbo.sysdiagrams set definition.write(0xA71D4DB8E79716E1E3D8E0341F1E3D1E048B38000E0080D41C00000000000000, null, 0) where diagram_id = @diagram_id; -- index:5633
    update dbo.sysdiagrams set definition.write(0x0000000200000005000000540000002C0000002C0000002C0000003400000000, null, 0) where diagram_id = @diagram_id; -- index:5665
    update dbo.sysdiagrams set definition.write(0x0000000000000022290000F3100000000000002D010000070000000C00000007, null, 0) where diagram_id = @diagram_id; -- index:5697
    update dbo.sysdiagrams set definition.write(0x0000001C0100000609000062070000480300001A040000DF020000EC04000027, null, 0) where diagram_id = @diagram_id; -- index:5729
    update dbo.sysdiagrams set definition.write(0x060000B103000027060000CB070000550500000000000001000000391300005B, null, 0) where diagram_id = @diagram_id; -- index:5761
    update dbo.sysdiagrams set definition.write(0x17000000000000030000000300000002000000020000001C0100001509000000, null, 0) where diagram_id = @diagram_id; -- index:5793
    update dbo.sysdiagrams set definition.write(0x0000000100000039130000220700000000000001000000010000000200000002, null, 0) where diagram_id = @diagram_id; -- index:5825
    update dbo.sysdiagrams set definition.write(0x0000001C01000015090000010000000000000039130000340300000000000000, null, 0) where diagram_id = @diagram_id; -- index:5857
    update dbo.sysdiagrams set definition.write(0x0000000000000002000000020000001C010000060900000000000000000000D1, null, 0) where diagram_id = @diagram_id; -- index:5889
    update dbo.sysdiagrams set definition.write(0x3100000923000000000000000000000D00000004000000040000001C01000006, null, 0) where diagram_id = @diagram_id; -- index:5921
    update dbo.sysdiagrams set definition.write(0x090000AA0A00009006000078563412040000006000000001000000010000000B, null, 0) where diagram_id = @diagram_id; -- index:5953
    update dbo.sysdiagrams set definition.write(0x0000000000000001000000020000000300000004000000050000000600000007, null, 0) where diagram_id = @diagram_id; -- index:5985
    update dbo.sysdiagrams set definition.write(0x00000008000000090000000A00000004000000640062006F0000000C00000070, null, 0) where diagram_id = @diagram_id; -- index:6017
    update dbo.sysdiagrams set definition.write(0x00650072006D0069007300730069006F006E007300000004000B0014ECFFFFBC, null, 0) where diagram_id = @diagram_id; -- index:6049
    update dbo.sysdiagrams set definition.write(0xE9FFFFB3E9FFFFBCE9FFFFB3E9FFFFB4E2FFFF97E2FFFFB4E2FFFF0000000002, null, 0) where diagram_id = @diagram_id; -- index:6081
    update dbo.sysdiagrams set definition.write(0x000000F0F0F00000000000000000000000000000000000010000001000000000, null, 0) where diagram_id = @diagram_id; -- index:6113
    update dbo.sysdiagrams set definition.write(0x0000004BE9FFFFADE0FFFFE20F0000580100003F000000010000020000E20F00, null, 0) where diagram_id = @diagram_id; -- index:6145
    update dbo.sysdiagrams set definition.write(0x0058010000020000000000050000800800008001000000150001000000900144, null, 0) where diagram_id = @diagram_id; -- index:6177
    update dbo.sysdiagrams set definition.write(0x420100065461686F6D611B0066006B005F005F0075007300650072005F007000, null, 0) where diagram_id = @diagram_id; -- index:6209
    update dbo.sysdiagrams set definition.write(0x650072006D0069007300730069006F006E0073005F005F007500730065007200, null, 0) where diagram_id = @diagram_id; -- index:6241
    update dbo.sysdiagrams set definition.write(0x730002000B00BED8FFFF86F2FFFFBED8FFFF13E9FFFF0000000002000000F0F0, null, 0) where diagram_id = @diagram_id; -- index:6273
    update dbo.sysdiagrams set definition.write(0xF000000000000000000000000000000000000100000012000000000000006DD9, null, 0) where diagram_id = @diagram_id; -- index:6305
    update dbo.sysdiagrams set definition.write(0xFFFFDCF0FFFF2613000058010000040000000100000200002613000058010000, null, 0) where diagram_id = @diagram_id; -- index:6337
    update dbo.sysdiagrams set definition.write(0x020000000000FFFFFF0008000080010000001500010000009001444201000654, null, 0) where diagram_id = @diagram_id; -- index:6369
    update dbo.sysdiagrams set definition.write(0x61686F6D61210066006B005F005F0075007300650072005F007000650072006D, null, 0) where diagram_id = @diagram_id; -- index:6401
    update dbo.sysdiagrams set definition.write(0x0069007300730069006F006E0073005F005F007000650072006D006900730073, null, 0) where diagram_id = @diagram_id; -- index:6433
    update dbo.sysdiagrams set definition.write(0x0069006F006E00730002000B0014ECFFFF16DBFFFF97E2FFFF16DBFFFF000000, null, 0) where diagram_id = @diagram_id; -- index:6465
    update dbo.sysdiagrams set definition.write(0x0002000000F0F0F0000000000000000000000000000000000001000000160000, null, 0) where diagram_id = @diagram_id; -- index:6497
    update dbo.sysdiagrams set definition.write(0x00000000009FDFFFFF0FD9FFFF6F0F000058010000320000000100000200006F, null, 0) where diagram_id = @diagram_id; -- index:6529
    update dbo.sysdiagrams set definition.write(0x0F000058010000020000000000FFFFFF00080000800100000015000100000090, null, 0) where diagram_id = @diagram_id; -- index:6561
    update dbo.sysdiagrams set definition.write(0x0144420100065461686F6D611B0066006B005F005F0075007300650072005F00, null, 0) where diagram_id = @diagram_id; -- index:6593
    update dbo.sysdiagrams set definition.write(0x7000650072006D0069007300730069006F006E0073005F005F00730069007400, null, 0) where diagram_id = @diagram_id; -- index:6625
    update dbo.sysdiagrams set definition.write(0x6500730000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6657
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6689
    update dbo.sysdiagrams set definition.write(0x0100FEFF030A0000FFFFFFFF0000000000000000000000000000000017000000, null, 0) where diagram_id = @diagram_id; -- index:6721
    update dbo.sysdiagrams set definition.write(0x4D6963726F736F66742044445320466F726D20322E300010000000456D626564, null, 0) where diagram_id = @diagram_id; -- index:6753
    update dbo.sysdiagrams set definition.write(0x646564204F626A6563740000000000F439B27100000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6785
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6817
    update dbo.sysdiagrams set definition.write(0x0C000000A0C4FFFF00CEFFFF0100260000007300630068005F006C0061006200, null, 0) where diagram_id = @diagram_id; -- index:6849
    update dbo.sysdiagrams set definition.write(0x65006C0073005F00760069007300690062006C0065000000010000000B000000, null, 0) where diagram_id = @diagram_id; -- index:6881
    update dbo.sysdiagrams set definition.write(0x1E00000000000000000000000000000000000000640000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6913
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000010000000100000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6945
    update dbo.sysdiagrams set definition.write(0x000000000000D002000006002800000041006300740069007600650054006100, null, 0) where diagram_id = @diagram_id; -- index:6977
    update dbo.sysdiagrams set definition.write(0x62006C00650056006900650077004D006F006400650000000100000008000400, null, 0) where diagram_id = @diagram_id; -- index:7009
    update dbo.sysdiagrams set definition.write(0x000032000000200000005400610062006C00650056006900650077004D006F00, null, 0) where diagram_id = @diagram_id; -- index:7041
    update dbo.sysdiagrams set definition.write(0x640065003A00300000000100000008003A00000034002C0030002C0032003800, null, 0) where diagram_id = @diagram_id; -- index:7073
    update dbo.sysdiagrams set definition.write(0x34002C0030002C0032003300310030002C0031002C0031003800390030002C00, null, 0) where diagram_id = @diagram_id; -- index:7105
    update dbo.sysdiagrams set definition.write(0x35002C0031003200360030000000200000005400610062006C00650056006900, null, 0) where diagram_id = @diagram_id; -- index:7137
    update dbo.sysdiagrams set definition.write(0x0300440064007300530074007200650061006D00000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7169
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7201
    update dbo.sysdiagrams set definition.write(0x160002000300000006000000FFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7233
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000000000000000000004B0000002C0B000000000000, null, 0) where diagram_id = @diagram_id; -- index:7265
    update dbo.sysdiagrams set definition.write(0x53006300680065006D0061002000550044005600200044006500660061007500, null, 0) where diagram_id = @diagram_id; -- index:7297
    update dbo.sysdiagrams set definition.write(0x6C00740000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7329
    update dbo.sysdiagrams set definition.write(0x26000200FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7361
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000780000001600000000000000, null, 0) where diagram_id = @diagram_id; -- index:7393
    update dbo.sysdiagrams set definition.write(0x440053005200450046002D0053004300480045004D0041002D0043004F004E00, null, 0) where diagram_id = @diagram_id; -- index:7425
    update dbo.sysdiagrams set definition.write(0x540045004E005400530000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7457
    update dbo.sysdiagrams set definition.write(0x2C0002010500000007000000FFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7489
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000790000003C03000000000000, null, 0) where diagram_id = @diagram_id; -- index:7521
    update dbo.sysdiagrams set definition.write(0x53006300680065006D0061002000550044005600200044006500660061007500, null, 0) where diagram_id = @diagram_id; -- index:7553
    update dbo.sysdiagrams set definition.write(0x6C007400200050006F0073007400200056003600000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7585
    update dbo.sysdiagrams set definition.write(0x36000200FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7617
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000860000001200000000000000, null, 0) where diagram_id = @diagram_id; -- index:7649
    update dbo.sysdiagrams set definition.write(0x650077004D006F00640065003A00310000000100000008001E00000032002C00, null, 0) where diagram_id = @diagram_id; -- index:7681
    update dbo.sysdiagrams set definition.write(0x30002C003200380034002C0030002C0032003800300035000000200000005400, null, 0) where diagram_id = @diagram_id; -- index:7713
    update dbo.sysdiagrams set definition.write(0x610062006C00650056006900650077004D006F00640065003A00320000000100, null, 0) where diagram_id = @diagram_id; -- index:7745
    update dbo.sysdiagrams set definition.write(0x000008001E00000032002C0030002C003200380034002C0030002C0031003700, null, 0) where diagram_id = @diagram_id; -- index:7777
    update dbo.sysdiagrams set definition.write(0x350035000000200000005400610062006C00650056006900650077004D006F00, null, 0) where diagram_id = @diagram_id; -- index:7809
    update dbo.sysdiagrams set definition.write(0x640065003A00330000000100000008001E00000032002C0030002C0032003800, null, 0) where diagram_id = @diagram_id; -- index:7841
    update dbo.sysdiagrams set definition.write(0x34002C0030002C0032003300310030000000200000005400610062006C006500, null, 0) where diagram_id = @diagram_id; -- index:7873
    update dbo.sysdiagrams set definition.write(0x56006900650077004D006F00640065003A00340000000100000008003E000000, null, 0) where diagram_id = @diagram_id; -- index:7905
    update dbo.sysdiagrams set definition.write(0x34002C0030002C003200380034002C0030002C0032003300310030002C003100, null, 0) where diagram_id = @diagram_id; -- index:7937
    update dbo.sysdiagrams set definition.write(0x32002C0032003700330030002C00310031002C00310036003800300000000200, null, 0) where diagram_id = @diagram_id; -- index:7969
    update dbo.sysdiagrams set definition.write(0x00000200000000000000000000000000000000000000D0020000060028000000, null, 0) where diagram_id = @diagram_id; -- index:8001
    update dbo.sysdiagrams set definition.write(0x4100630074006900760065005400610062006C00650056006900650077004D00, null, 0) where diagram_id = @diagram_id; -- index:8033
    update dbo.sysdiagrams set definition.write(0x6F00640065000000010000000800040000003200000020000000540061006200, null, 0) where diagram_id = @diagram_id; -- index:8065
    update dbo.sysdiagrams set definition.write(0x6C00650056006900650077004D006F00640065003A0030000000010000000800, null, 0) where diagram_id = @diagram_id; -- index:8097
    update dbo.sysdiagrams set definition.write(0x3A00000034002C0030002C003200380034002C0030002C003200330031003000, null, 0) where diagram_id = @diagram_id; -- index:8129
    update dbo.sysdiagrams set definition.write(0x2C0031002C0031003800390030002C0035002C00310032003600300000002000, null, 0) where diagram_id = @diagram_id; -- index:8161
    update dbo.sysdiagrams set definition.write(0x00005400610062006C00650056006900650077004D006F00640065003A003100, null, 0) where diagram_id = @diagram_id; -- index:8193
    update dbo.sysdiagrams set definition.write(0x00000100000008001E00000032002C0030002C003200380034002C0030002C00, null, 0) where diagram_id = @diagram_id; -- index:8225
    update dbo.sysdiagrams set definition.write(0x32003800300035000000200000005400610062006C0065005600690065007700, null, 0) where diagram_id = @diagram_id; -- index:8257
    update dbo.sysdiagrams set definition.write(0x4D006F00640065003A00320000000100000008001E00000032002C0030002C00, null, 0) where diagram_id = @diagram_id; -- index:8289
    update dbo.sysdiagrams set definition.write(0x3200380034002C0030002C003100370035003500000020000000540061006200, null, 0) where diagram_id = @diagram_id; -- index:8321
    update dbo.sysdiagrams set definition.write(0x6C00650056006900650077004D006F00640065003A0033000000010000000800, null, 0) where diagram_id = @diagram_id; -- index:8353
    update dbo.sysdiagrams set definition.write(0x1E00000032002C0030002C003200380034002C0030002C003200330031003000, null, 0) where diagram_id = @diagram_id; -- index:8385
    update dbo.sysdiagrams set definition.write(0x0000200000005400610062006C00650056006900650077004D006F0064006500, null, 0) where diagram_id = @diagram_id; -- index:8417
    update dbo.sysdiagrams set definition.write(0x3A00340000000100000008003E00000034002C0030002C003200380034002C00, null, 0) where diagram_id = @diagram_id; -- index:8449
    update dbo.sysdiagrams set definition.write(0x30002C0032003300310030002C00310032002C0032003700330030002C003100, null, 0) where diagram_id = @diagram_id; -- index:8481
    update dbo.sysdiagrams set definition.write(0x31002C003100360038003000000007000000070000000000000032000000011C, null, 0) where diagram_id = @diagram_id; -- index:8513
    update dbo.sysdiagrams set definition.write(0xFD5C01000000640062006F00000066006B005F005F0075007300650072007300, null, 0) where diagram_id = @diagram_id; -- index:8545
    update dbo.sysdiagrams set definition.write(0x5F005F007300690074006500730000000000000000000000C402000000000800, null, 0) where diagram_id = @diagram_id; -- index:8577
    update dbo.sysdiagrams set definition.write(0x000008000000070000000800000001A0BA3C40A0BA3C0000000000000000AD07, null, 0) where diagram_id = @diagram_id; -- index:8609
    update dbo.sysdiagrams set definition.write(0x0000000000090000000900000000000000000000000000000000000000D00200, null, 0) where diagram_id = @diagram_id; -- index:8641
    update dbo.sysdiagrams set definition.write(0x000600280000004100630074006900760065005400610062006C006500560069, null, 0) where diagram_id = @diagram_id; -- index:8673
    update dbo.sysdiagrams set definition.write(0x00650077004D006F006400650000000100000008000400000032000000200000, null, 0) where diagram_id = @diagram_id; -- index:8705
    update dbo.sysdiagrams set definition.write(0x005400610062006C00650056006900650077004D006F00640065003A00300000, null, 0) where diagram_id = @diagram_id; -- index:8737
    update dbo.sysdiagrams set definition.write(0x000100000008003A00000034002C0030002C003200380034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:8769
    update dbo.sysdiagrams set definition.write(0x003300310030002C0031002C0031003800390030002C0035002C003100320036, null, 0) where diagram_id = @diagram_id; -- index:8801
    update dbo.sysdiagrams set definition.write(0x0030000000200000005400610062006C00650056006900650077004D006F0064, null, 0) where diagram_id = @diagram_id; -- index:8833
    update dbo.sysdiagrams set definition.write(0x0065003A00310000000100000008001E00000032002C0030002C003200380034, null, 0) where diagram_id = @diagram_id; -- index:8865
    update dbo.sysdiagrams set definition.write(0x002C0030002C0032003300320035000000200000005400610062006C00650056, null, 0) where diagram_id = @diagram_id; -- index:8897
    update dbo.sysdiagrams set definition.write(0x006900650077004D006F00640065003A00320000000100000008001E00000032, null, 0) where diagram_id = @diagram_id; -- index:8929
    update dbo.sysdiagrams set definition.write(0x002C0030002C003200380034002C0030002C0032003300320035000000200000, null, 0) where diagram_id = @diagram_id; -- index:8961
    update dbo.sysdiagrams set definition.write(0x005400610062006C00650056006900650077004D006F00640065003A00330000, null, 0) where diagram_id = @diagram_id; -- index:8993
    update dbo.sysdiagrams set definition.write(0x000100000008001E00000032002C0030002C003200380034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:9025
    update dbo.sysdiagrams set definition.write(0x003300310030000000200000005400610062006C00650056006900650077004D, null, 0) where diagram_id = @diagram_id; -- index:9057
    update dbo.sysdiagrams set definition.write(0x006F00640065003A00340000000100000008003E00000034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:9089
    update dbo.sysdiagrams set definition.write(0x00380034002C0030002C0032003300310030002C00310032002C003200370033, null, 0) where diagram_id = @diagram_id; -- index:9121
    update dbo.sysdiagrams set definition.write(0x0030002C00310031002C00310036003800300000000A0000000A000000000000, null, 0) where diagram_id = @diagram_id; -- index:9153
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000000D0020000060028000000410063007400690076, null, 0) where diagram_id = @diagram_id; -- index:9185
    update dbo.sysdiagrams set definition.write(0x0065005400610062006C00650056006900650077004D006F0064006500000001, null, 0) where diagram_id = @diagram_id; -- index:9217
    update dbo.sysdiagrams set definition.write(0x00000008000400000032000000200000005400610062006C0065005600690065, null, 0) where diagram_id = @diagram_id; -- index:9249
    update dbo.sysdiagrams set definition.write(0x0077004D006F00640065003A00300000000100000008003A00000034002C0030, null, 0) where diagram_id = @diagram_id; -- index:9281
    update dbo.sysdiagrams set definition.write(0x002C003200380034002C0030002C0032003300310030002C0031002C00310038, null, 0) where diagram_id = @diagram_id; -- index:9313
    update dbo.sysdiagrams set definition.write(0x00390030002C0035002C0031003200360030000000200000005400610062006C, null, 0) where diagram_id = @diagram_id; -- index:9345
    update dbo.sysdiagrams set definition.write(0x00650056006900650077004D006F00640065003A00310000000100000008001E, null, 0) where diagram_id = @diagram_id; -- index:9377
    update dbo.sysdiagrams set definition.write(0x00000032002C0030002C003200380034002C0030002C00320033003200350000, null, 0) where diagram_id = @diagram_id; -- index:9409
    update dbo.sysdiagrams set definition.write(0x00200000005400610062006C00650056006900650077004D006F00640065003A, null, 0) where diagram_id = @diagram_id; -- index:9441
    update dbo.sysdiagrams set definition.write(0x00320000000100000008001E00000032002C0030002C003200380034002C0030, null, 0) where diagram_id = @diagram_id; -- index:9473
    update dbo.sysdiagrams set definition.write(0x002C0032003300320035000000200000005400610062006C0065005600690065, null, 0) where diagram_id = @diagram_id; -- index:9505
    update dbo.sysdiagrams set definition.write(0x0077004D006F00640065003A00330000000100000008001E00000032002C0030, null, 0) where diagram_id = @diagram_id; -- index:9537
    update dbo.sysdiagrams set definition.write(0x002C003200380034002C0030002C003200330031003000000020000000540061, null, 0) where diagram_id = @diagram_id; -- index:9569
    update dbo.sysdiagrams set definition.write(0x0062006C00650056006900650077004D006F00640065003A0034000000010000, null, 0) where diagram_id = @diagram_id; -- index:9601
    update dbo.sysdiagrams set definition.write(0x0008003E00000034002C0030002C003200380034002C0030002C003200330031, null, 0) where diagram_id = @diagram_id; -- index:9633
    update dbo.sysdiagrams set definition.write(0x0030002C00310032002C0032003700330030002C00310031002C003100360038, null, 0) where diagram_id = @diagram_id; -- index:9665
    update dbo.sysdiagrams set definition.write(0x00300000000F0000000F00000000000000480000000100181801000000640062, null, 0) where diagram_id = @diagram_id; -- index:9697
    update dbo.sysdiagrams set definition.write(0x006F00000066006B005F005F0075007300650072005F007000650072006D0069, null, 0) where diagram_id = @diagram_id; -- index:9729
    update dbo.sysdiagrams set definition.write(0x007300730069006F006E0073005F005F00750073006500720073000000000000, null, 0) where diagram_id = @diagram_id; -- index:9761
    update dbo.sysdiagrams set definition.write(0x0000000000C4020000000010000000100000000F0000000800000001A3BA3C80, null, 0) where diagram_id = @diagram_id; -- index:9793
    update dbo.sysdiagrams set definition.write(0xA3BA3C0000000000000000AD0700000000001100000011000000000000005400, null, 0) where diagram_id = @diagram_id; -- index:9825
    update dbo.sysdiagrams set definition.write(0x00000111257601000000640062006F00000066006B005F005F00750073006500, null, 0) where diagram_id = @diagram_id; -- index:9857
    update dbo.sysdiagrams set definition.write(0x72005F007000650072006D0069007300730069006F006E0073005F005F007000, null, 0) where diagram_id = @diagram_id; -- index:9889
    update dbo.sysdiagrams set definition.write(0x650072006D0069007300730069006F006E00730000000000000000000000C402, null, 0) where diagram_id = @diagram_id; -- index:9921
    update dbo.sysdiagrams set definition.write(0x0000000012000000120000001100000008000000019EBA3CC09EBA3C00000000, null, 0) where diagram_id = @diagram_id; -- index:9953
    update dbo.sysdiagrams set definition.write(0x00000000AD070000000000150000001500000000000000480000000100270001, null, 0) where diagram_id = @diagram_id; -- index:9985
    update dbo.sysdiagrams set definition.write(0x000000640062006F00000066006B005F005F0075007300650072005F00700065, null, 0) where diagram_id = @diagram_id; -- index:10017
    update dbo.sysdiagrams set definition.write(0x0072006D0069007300730069006F006E0073005F005F00730069007400650073, null, 0) where diagram_id = @diagram_id; -- index:10049
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000C40200000000160000001600000015000000080000, null, 0) where diagram_id = @diagram_id; -- index:10081
    update dbo.sysdiagrams set definition.write(0x0001509F3C78509F3C0000000000000000AD0F00000100001400000015000000, null, 0) where diagram_id = @diagram_id; -- index:10113
    update dbo.sysdiagrams set definition.write(0x0100000009000000460000004100000007000000010000000200000019000000, null, 0) where diagram_id = @diagram_id; -- index:10145
    update dbo.sysdiagrams set definition.write(0x180000000F0000000200000009000000400000005B000000110000000A000000, null, 0) where diagram_id = @diagram_id; -- index:10177
    update dbo.sysdiagrams set definition.write(0x090000001E0000001F0000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:10209
    update dbo.sysdiagrams set definition.write(0x010003000000000000000C0000000B0000004E61BC0000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:10241
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:10273
    update dbo.sysdiagrams set definition.write(0xDBE6B0E91C81D011AD5100A0C90F573900000200F0BCD088B548D60102020000, null, 0) where diagram_id = @diagram_id; -- index:10305
    update dbo.sysdiagrams set definition.write(0x1048450000000000000000000000000000000000E00100004400610074006100, null, 0) where diagram_id = @diagram_id; -- index:10337
    update dbo.sysdiagrams set definition.write(0x200053006F0075007200630065003D0039003700540047005100560032005C00, null, 0) where diagram_id = @diagram_id; -- index:10369
    update dbo.sysdiagrams set definition.write(0x4D00590032003000310036003B0049006E0069007400690061006C0020004300, null, 0) where diagram_id = @diagram_id; -- index:10401
    update dbo.sysdiagrams set definition.write(0x6100740061006C006F0067003D0065006D00610072003B005000650072007300, null, 0) where diagram_id = @diagram_id; -- index:10433
    update dbo.sysdiagrams set definition.write(0x690073007400200053006500630075007200690074007900200049006E006600, null, 0) where diagram_id = @diagram_id; -- index:10465
    update dbo.sysdiagrams set definition.write(0x6F003D0054007200750065003B0055007300650072002000490044003D007300, null, 0) where diagram_id = @diagram_id; -- index:10497
    update dbo.sysdiagrams set definition.write(0x61003B004D0075006C007400690070006C006500410063007400690076006500, null, 0) where diagram_id = @diagram_id; -- index:10529
    update dbo.sysdiagrams set definition.write(0x52006500730075006C00740053006500740073003D00460061006C0073006500, null, 0) where diagram_id = @diagram_id; -- index:10561
    update dbo.sysdiagrams set definition.write(0x3B0043006F006E006E006500630074002000540069006D0065006F0075007400, null, 0) where diagram_id = @diagram_id; -- index:10593
    update dbo.sysdiagrams set definition.write(0x3D00330030003B00540072007500730074005300650072007600650072004300, null, 0) where diagram_id = @diagram_id; -- index:10625
    update dbo.sysdiagrams set definition.write(0x65007200740069006600690063006100740065003D00460061006C0073006500, null, 0) where diagram_id = @diagram_id; -- index:10657
    update dbo.sysdiagrams set definition.write(0x3B005000610063006B00650074002000530069007A0065003D00340030003900, null, 0) where diagram_id = @diagram_id; -- index:10689
    update dbo.sysdiagrams set definition.write(0x36003B004100700070006C00690063006100740069006F006E0020004E006100, null, 0) where diagram_id = @diagram_id; -- index:10721
    update dbo.sysdiagrams set definition.write(0x8100000082000000830000008400000085000000FEFFFFFFFEFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10753
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10785
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10817
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10849
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10881
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10913
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10945
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:10977
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11009
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11041
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11073
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11105
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11137
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11169
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11201
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:11233
    update dbo.sysdiagrams set definition.write(0x6D0065003D0022004D006900630072006F0073006F0066007400200053005100, null, 0) where diagram_id = @diagram_id; -- index:11265
    update dbo.sysdiagrams set definition.write(0x4C00200053006500720076006500720020004D0061006E006100670065006D00, null, 0) where diagram_id = @diagram_id; -- index:11297
    update dbo.sysdiagrams set definition.write(0x65006E0074002000530074007500640069006F00220000000080050012000000, null, 0) where diagram_id = @diagram_id; -- index:11329
    update dbo.sysdiagrams set definition.write(0x5300650063007500720069007400790000000002260018000000700065007200, null, 0) where diagram_id = @diagram_id; -- index:11361
    update dbo.sysdiagrams set definition.write(0x6D0069007300730069006F006E007300000008000000640062006F0000000002, null, 0) where diagram_id = @diagram_id; -- index:11393
    update dbo.sysdiagrams set definition.write(0x26002200000075007300650072005F007000650072006D006900730073006900, null, 0) where diagram_id = @diagram_id; -- index:11425
    update dbo.sysdiagrams set definition.write(0x6F006E007300000008000000640062006F000000000226000C00000075007300, null, 0) where diagram_id = @diagram_id; -- index:11457
    update dbo.sysdiagrams set definition.write(0x650072007300000008000000640062006F000000000224000C00000073006900, null, 0) where diagram_id = @diagram_id; -- index:11489
    update dbo.sysdiagrams set definition.write(0x740065007300000008000000640062006F00000001000000D68509B3BB6BF245, null, 0) where diagram_id = @diagram_id; -- index:11521
    update dbo.sysdiagrams set definition.write(0x9AB8371664F0327008004E0000007B0031003600330034004300440044003700, null, 0) where diagram_id = @diagram_id; -- index:11553
    update dbo.sysdiagrams set definition.write(0x2D0030003800380038002D0034003200450033002D0039004600410032002D00, null, 0) where diagram_id = @diagram_id; -- index:11585
    update dbo.sysdiagrams set definition.write(0x4200360044003300320035003600330042003900310044007D00000000000000, null, 0) where diagram_id = @diagram_id; -- index:11617
    update dbo.sysdiagrams set definition.write(0x010003000000000000000C0000000B0000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:11649
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:11681
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:11713
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:11745
    update dbo.sysdiagrams set definition.write(0x62885214, null, 0) where diagram_id = @diagram_id; -- index:11777

    print '=== Diagram [Security] restored at diagram_id=' + cast(@diagram_id as varchar(10)) + '. ===';
end try
begin catch
    delete from dbo.sysdiagrams where diagram_id = @diagram_id;
    print '=== ' + error_message() + ' ===';
end catch;
-- End of restore diagram [Security] script.
end;
