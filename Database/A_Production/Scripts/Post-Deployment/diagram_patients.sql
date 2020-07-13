
set    @continue_update = 0;
select @continue_update = 1
from [sys].[tables]
where [name] = 'sysdiagrams';

-------------------------------------------------------------------------
-- Summary: Restore diagram [Patients] from database [emar].
-------------------------------------------------------------------------
print '=== Restoring diagram [Patients] ===';

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
    where [name] = 'Patients';

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
            values('Patients', 1, @version, 0x);

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
    update dbo.sysdiagrams set definition.write(0x01000000FEFFFFFF0000000000000000FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:65
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
    update dbo.sysdiagrams set definition.write(0xFDFFFFFF10000000FEFFFFFF0400000005000000060000001100000008000000, null, 0) where diagram_id = @diagram_id; -- index:513
    update dbo.sysdiagrams set definition.write(0x090000000A0000000B0000000C0000000D0000000E0000000F000000FEFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:545
    update dbo.sysdiagrams set definition.write(0xFEFFFFFF12000000130000001400000015000000160000001700000018000000, null, 0) where diagram_id = @diagram_id; -- index:577
    update dbo.sysdiagrams set definition.write(0x19000000FEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:609
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
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000060A42858B448D601030000008019000000000000, null, 0) where diagram_id = @diagram_id; -- index:1121
    update dbo.sysdiagrams set definition.write(0x6600000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1153
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1185
    update dbo.sysdiagrams set definition.write(0x04000201FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1217
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000002206000000000000, null, 0) where diagram_id = @diagram_id; -- index:1249
    update dbo.sysdiagrams set definition.write(0x6F00000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1281
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1313
    update dbo.sysdiagrams set definition.write(0x040002010100000004000000FFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1345
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000000000000000007000000D811000000000000, null, 0) where diagram_id = @diagram_id; -- index:1377
    update dbo.sysdiagrams set definition.write(0x010043006F006D0070004F0062006A0000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1409
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1441
    update dbo.sysdiagrams set definition.write(0x12000201FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:1473
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000190000005F00000000000000, null, 0) where diagram_id = @diagram_id; -- index:1505
    update dbo.sysdiagrams set definition.write(0x0100000002000000030000000400000005000000060000000700000008000000, null, 0) where diagram_id = @diagram_id; -- index:1537
    update dbo.sysdiagrams set definition.write(0x090000000A0000000B0000000C0000000D0000000E0000000F00000010000000, null, 0) where diagram_id = @diagram_id; -- index:1569
    update dbo.sysdiagrams set definition.write(0x1100000012000000130000001400000015000000160000001700000018000000, null, 0) where diagram_id = @diagram_id; -- index:1601
    update dbo.sysdiagrams set definition.write(0xFEFFFFFF1A000000FEFFFFFF1C0000001D0000001E0000001F00000020000000, null, 0) where diagram_id = @diagram_id; -- index:1633
    update dbo.sysdiagrams set definition.write(0x2100000022000000230000002400000025000000260000002700000028000000, null, 0) where diagram_id = @diagram_id; -- index:1665
    update dbo.sysdiagrams set definition.write(0x290000002A0000002B0000002C0000002D0000002E0000002F00000030000000, null, 0) where diagram_id = @diagram_id; -- index:1697
    update dbo.sysdiagrams set definition.write(0x3100000032000000330000003400000035000000360000003700000038000000, null, 0) where diagram_id = @diagram_id; -- index:1729
    update dbo.sysdiagrams set definition.write(0x390000003A0000003B0000003C0000003D0000003E0000003F00000040000000, null, 0) where diagram_id = @diagram_id; -- index:1761
    update dbo.sysdiagrams set definition.write(0x4100000042000000430000004400000045000000460000004700000048000000, null, 0) where diagram_id = @diagram_id; -- index:1793
    update dbo.sysdiagrams set definition.write(0x490000004A0000004B0000004C0000004D0000004E0000004F00000050000000, null, 0) where diagram_id = @diagram_id; -- index:1825
    update dbo.sysdiagrams set definition.write(0x5100000052000000530000005400000055000000FEFFFFFFFEFFFFFF58000000, null, 0) where diagram_id = @diagram_id; -- index:1857
    update dbo.sysdiagrams set definition.write(0x590000005A0000005B0000005C0000005D0000005E0000005F00000060000000, null, 0) where diagram_id = @diagram_id; -- index:1889
    update dbo.sysdiagrams set definition.write(0x61000000620000006300000064000000FEFFFFFFFEFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:1921
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:1953
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:1985
    update dbo.sysdiagrams set definition.write(0xFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF, null, 0) where diagram_id = @diagram_id; -- index:2017
    update dbo.sysdiagrams set definition.write(0x000430000A1E100C05000080320000000F00FFFF32000000007D0000339D0000, null, 0) where diagram_id = @diagram_id; -- index:2049
    update dbo.sysdiagrams set definition.write(0xFC5B00003BA400003064000044E4FFFF3ECCFFFFDE805B10F195D011B0A000AA, null, 0) where diagram_id = @diagram_id; -- index:2081
    update dbo.sysdiagrams set definition.write(0x00BDCB5C000008003000000000020000030000003C006B000000090000000000, null, 0) where diagram_id = @diagram_id; -- index:2113
    update dbo.sysdiagrams set definition.write(0x0000D9E6B0E91C81D011AD5100A0C90F5739F43B7F847F61C74385352986E1D5, null, 0) where diagram_id = @diagram_id; -- index:2145
    update dbo.sysdiagrams set definition.write(0x52F8A0327DB2D86295428D98273C25A2DA2D000028004300000000000000B5B0, null, 0) where diagram_id = @diagram_id; -- index:2177
    update dbo.sysdiagrams set definition.write(0xC832B618F5469CA7016F91DF3A0134C9D2777977D811907000065B840D9C0000, null, 0) where diagram_id = @diagram_id; -- index:2209
    update dbo.sysdiagrams set definition.write(0x280043000000000000007B31EFF5E6FA56429865BD40E34CA05E34C9D2777977, null, 0) where diagram_id = @diagram_id; -- index:2241
    update dbo.sysdiagrams set definition.write(0xD811907000065B840D9C11000000300500000091012C00003C00A50900000700, null, 0) where diagram_id = @diagram_id; -- index:2273
    update dbo.sysdiagrams set definition.write(0x008001000000B60200000080000012000080536368477269640002EFFFFFD0D5, null, 0) where diagram_id = @diagram_id; -- index:2305
    update dbo.sysdiagrams set definition.write(0xFFFF70617469656E745F696E64696361746F7273000000003000A50900000700, null, 0) where diagram_id = @diagram_id; -- index:2337
    update dbo.sysdiagrams set definition.write(0x008002000000A202000000800000080000805363684772696400B80B0000FCD6, null, 0) where diagram_id = @diagram_id; -- index:2369
    update dbo.sysdiagrams set definition.write(0xFFFF70617469656E747300008400A50900000700008005000000520000000180, null, 0) where diagram_id = @diagram_id; -- index:2401
    update dbo.sysdiagrams set definition.write(0x00005B000080436F6E74726F6C000F01000015DAFFFF52656C6174696F6E7368, null, 0) where diagram_id = @diagram_id; -- index:2433
    update dbo.sysdiagrams set definition.write(0x69702027666B5F5F70617469656E745F696E64696361746F72735F5F70617469, null, 0) where diagram_id = @diagram_id; -- index:2465
    update dbo.sysdiagrams set definition.write(0x656E747327206265747765656E202770617469656E74732720616E6420277061, null, 0) where diagram_id = @diagram_id; -- index:2497
    update dbo.sysdiagrams set definition.write(0x7469656E745F696E64696361746F7273270000002800B5010000070000800600, null, 0) where diagram_id = @diagram_id; -- index:2529
    update dbo.sysdiagrams set definition.write(0x0000310000007300000002800000436F6E74726F6C0014FEFFFFA5D9FFFF0000, null, 0) where diagram_id = @diagram_id; -- index:2561
    update dbo.sysdiagrams set definition.write(0x3400A5090000070000800D000000AA020000008000000C000080536368477269, null, 0) where diagram_id = @diagram_id; -- index:2593
    update dbo.sysdiagrams set definition.write(0x640012FDFFFF3EFEFFFF65787465726E616C5F69647300003800A50900000700, null, 0) where diagram_id = @diagram_id; -- index:2625
    update dbo.sysdiagrams set definition.write(0x00800E000000B202000000800000100000805363684772696400C81900007EEB, null, 0) where diagram_id = @diagram_id; -- index:2657
    update dbo.sysdiagrams set definition.write(0xFFFF736974655F636F64655F73686172657300003000A5090000070000801000, null, 0) where diagram_id = @diagram_id; -- index:2689
    update dbo.sysdiagrams set definition.write(0x00009C0200000080000005000080536368477269640012FDFFFFD6EDFFFF7369, null, 0) where diagram_id = @diagram_id; -- index:2721
    update dbo.sysdiagrams set definition.write(0x74657369640000007400A5090000070000801A00000052000000018000004900, null, 0) where diagram_id = @diagram_id; -- index:2753
    update dbo.sysdiagrams set definition.write(0x0080436F6E74726F6C00DB04000041F2FFFF52656C6174696F6E736869702027, null, 0) where diagram_id = @diagram_id; -- index:2785
    update dbo.sysdiagrams set definition.write(0x666B5F5F65787465726E616C5F6964735F5F736974657327206265747765656E, null, 0) where diagram_id = @diagram_id; -- index:2817
    update dbo.sysdiagrams set definition.write(0x202773697465732720616E64202765787465726E616C5F69647327007D000000, null, 0) where diagram_id = @diagram_id; -- index:2849
    update dbo.sysdiagrams set definition.write(0x2800B5010000070000801B000000310000006100000002800000436F6E74726F, null, 0) where diagram_id = @diagram_id; -- index:2881
    update dbo.sysdiagrams set definition.write(0x6C00ECF8FFFFEFF8FFFF00008800A5090000070000801C000000520000000180, null, 0) where diagram_id = @diagram_id; -- index:2913
    update dbo.sysdiagrams set definition.write(0x000060000080436F6E74726F6C731F0F0000C3EFFFFF52656C6174696F6E7368, null, 0) where diagram_id = @diagram_id; -- index:2945
    update dbo.sysdiagrams set definition.write(0x69702027666B5F5F736974655F636F64655F7368617265735F5F73697465735F, null, 0) where diagram_id = @diagram_id; -- index:2977
    update dbo.sysdiagrams set definition.write(0x5F73686172655F736974655F696427206265747765656E202773697465732720, null, 0) where diagram_id = @diagram_id; -- index:3009
    update dbo.sysdiagrams set definition.write(0x616E642027736974655F636F64655F7368617265732700002800B50100000700, null, 0) where diagram_id = @diagram_id; -- index:3041
    update dbo.sysdiagrams set definition.write(0x00801D000000310000008700000002800000436F6E74726F6C739106000009F2, null, 0) where diagram_id = @diagram_id; -- index:3073
    update dbo.sysdiagrams set definition.write(0xFFFF00008400A5090000070000801E00000052000000018000005A000080436F, null, 0) where diagram_id = @diagram_id; -- index:3105
    update dbo.sysdiagrams set definition.write(0x6E74726F6C001F0F000059F0FFFF52656C6174696F6E736869702027666B5F5F, null, 0) where diagram_id = @diagram_id; -- index:3137
    update dbo.sysdiagrams set definition.write(0x736974655F636F64655F7368617265735F5F73697465735F5F736974655F6964, null, 0) where diagram_id = @diagram_id; -- index:3169
    update dbo.sysdiagrams set definition.write(0x27206265747765656E202773697465732720616E642027736974655F636F6465, null, 0) where diagram_id = @diagram_id; -- index:3201
    update dbo.sysdiagrams set definition.write(0x5F73686172657327000000002800B5010000070000801F000000310000007B00, null, 0) where diagram_id = @diagram_id; -- index:3233
    update dbo.sysdiagrams set definition.write(0x000002800000436F6E74726F6C00EA0800009FF2FFFF00008000A50900000700, null, 0) where diagram_id = @diagram_id; -- index:3265
    update dbo.sysdiagrams set definition.write(0x00802F000000520000000180000055000080436F6E74726F6C0011FCFFFF2DDF, null, 0) where diagram_id = @diagram_id; -- index:3297
    update dbo.sysdiagrams set definition.write(0xFFFF52656C6174696F6E736869702027666B5F5F70617469656E745F696E6469, null, 0) where diagram_id = @diagram_id; -- index:3329
    update dbo.sysdiagrams set definition.write(0x6361746F72735F5F736974657327206265747765656E20277369746573272061, null, 0) where diagram_id = @diagram_id; -- index:3361
    update dbo.sysdiagrams set definition.write(0x6E64202770617469656E745F696E64696361746F72732700000000002800B501, null, 0) where diagram_id = @diagram_id; -- index:3393
    update dbo.sysdiagrams set definition.write(0x00000700008030000000310000006D00000002800000436F6E74726F6C0057FE, null, 0) where diagram_id = @diagram_id; -- index:3425
    update dbo.sysdiagrams set definition.write(0xFFFF33E7FFFF00006C00A5090000070000803100000052000000018000004100, null, 0) where diagram_id = @diagram_id; -- index:3457
    update dbo.sysdiagrams set definition.write(0x0080436F6E74726F6C00B70A0000DEDDFFFF52656C6174696F6E736869702027, null, 0) where diagram_id = @diagram_id; -- index:3489
    update dbo.sysdiagrams set definition.write(0x666B5F5F70617469656E74735F5F736974657327206265747765656E20277369, null, 0) where diagram_id = @diagram_id; -- index:3521
    update dbo.sysdiagrams set definition.write(0x7465732720616E64202770617469656E74732700000000002800B50100000700, null, 0) where diagram_id = @diagram_id; -- index:3553
    update dbo.sysdiagrams set definition.write(0x008032000000310000005900000002800000436F6E74726F6C00FD0C00008CE6, null, 0) where diagram_id = @diagram_id; -- index:3585
    update dbo.sysdiagrams set definition.write(0xFFFF000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3617
    update dbo.sysdiagrams set definition.write(0x0100FEFF030A0000FFFFFFFF0000000000000000000000000000000017000000, null, 0) where diagram_id = @diagram_id; -- index:3649
    update dbo.sysdiagrams set definition.write(0x4D6963726F736F66742044445320466F726D20322E300010000000456D626564, null, 0) where diagram_id = @diagram_id; -- index:3681
    update dbo.sysdiagrams set definition.write(0x646564204F626A6563740000000000F439B27100000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3713
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3745
    update dbo.sysdiagrams set definition.write(0x0C00000044E4FFFF3ECCFFFF0100260000007300630068005F006C0061006200, null, 0) where diagram_id = @diagram_id; -- index:3777
    update dbo.sysdiagrams set definition.write(0x65006C0073005F00760069007300690062006C0065000000010000000B000000, null, 0) where diagram_id = @diagram_id; -- index:3809
    update dbo.sysdiagrams set definition.write(0x1E00000000000000000000000000000000000000640000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3841
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000010000000100000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:3873
    update dbo.sysdiagrams set definition.write(0x000000000000D002000006002800000041006300740069007600650054006100, null, 0) where diagram_id = @diagram_id; -- index:3905
    update dbo.sysdiagrams set definition.write(0x62006C00650056006900650077004D006F006400650000000100000008000400, null, 0) where diagram_id = @diagram_id; -- index:3937
    update dbo.sysdiagrams set definition.write(0x000032000000200000005400610062006C00650056006900650077004D006F00, null, 0) where diagram_id = @diagram_id; -- index:3969
    update dbo.sysdiagrams set definition.write(0x640065003A00300000000100000008003A00000034002C0030002C0032003800, null, 0) where diagram_id = @diagram_id; -- index:4001
    update dbo.sysdiagrams set definition.write(0x34002C0030002C0032003300310030002C0031002C0031003800390030002C00, null, 0) where diagram_id = @diagram_id; -- index:4033
    update dbo.sysdiagrams set definition.write(0x35002C0031003200360030000000200000005400610062006C00650056006900, null, 0) where diagram_id = @diagram_id; -- index:4065
    update dbo.sysdiagrams set definition.write(0x214334120800000039130000180C000078563412070000001401000070006100, null, 0) where diagram_id = @diagram_id; -- index:4097
    update dbo.sysdiagrams set definition.write(0x7400690065006E0074005F0069006E00640069006300610074006F0072007300, null, 0) where diagram_id = @diagram_id; -- index:4129
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4161
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4193
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4225
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4257
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4289
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4321
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000020000000500000054000000, null, 0) where diagram_id = @diagram_id; -- index:4353
    update dbo.sysdiagrams set definition.write(0x2C0000002C0000002C0000003400000000000000000000002229000050220000, null, 0) where diagram_id = @diagram_id; -- index:4385
    update dbo.sysdiagrams set definition.write(0x000000002D0100000A0000000C000000070000001C0100000609000062070000, null, 0) where diagram_id = @diagram_id; -- index:4417
    update dbo.sysdiagrams set definition.write(0x480300001A040000DF020000EC04000027060000B103000027060000CB070000, null, 0) where diagram_id = @diagram_id; -- index:4449
    update dbo.sysdiagrams set definition.write(0x550500000000000001000000881600007F180000000000000800000008000000, null, 0) where diagram_id = @diagram_id; -- index:4481
    update dbo.sysdiagrams set definition.write(0x02000000020000001C010000F50A0000000000000100000039130000180C0000, null, 0) where diagram_id = @diagram_id; -- index:4513
    update dbo.sysdiagrams set definition.write(0x00000000030000000300000002000000020000001C0100001509000001000000, null, 0) where diagram_id = @diagram_id; -- index:4545
    update dbo.sysdiagrams set definition.write(0x0000000039130000340300000000000000000000000000000200000002000000, null, 0) where diagram_id = @diagram_id; -- index:4577
    update dbo.sysdiagrams set definition.write(0x1C010000060900000000000000000000D1310000092300000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4609
    update dbo.sysdiagrams set definition.write(0x0D00000004000000040000001C01000006090000AA0A00009006000078563412, null, 0) where diagram_id = @diagram_id; -- index:4641
    update dbo.sysdiagrams set definition.write(0x040000006E00000001000000010000000B000000000000000100000002000000, null, 0) where diagram_id = @diagram_id; -- index:4673
    update dbo.sysdiagrams set definition.write(0x030000000400000005000000060000000700000008000000090000000A000000, null, 0) where diagram_id = @diagram_id; -- index:4705
    update dbo.sysdiagrams set definition.write(0x04000000640062006F00000013000000700061007400690065006E0074005F00, null, 0) where diagram_id = @diagram_id; -- index:4737
    update dbo.sysdiagrams set definition.write(0x69006E00640069006300610074006F0072007300000021433412080000003913, null, 0) where diagram_id = @diagram_id; -- index:4769
    update dbo.sysdiagrams set definition.write(0x00009D090000785634120700000014010000700061007400690065006E007400, null, 0) where diagram_id = @diagram_id; -- index:4801
    update dbo.sysdiagrams set definition.write(0x7300000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4833
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4865
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4897
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4929
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4961
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:4993
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5025
    update dbo.sysdiagrams set definition.write(0x000000000000000000000200000005000000540000002C0000002C0000002C00, null, 0) where diagram_id = @diagram_id; -- index:5057
    update dbo.sysdiagrams set definition.write(0x0000340000000000000000000000222900002B740000000000002D0100000D00, null, 0) where diagram_id = @diagram_id; -- index:5089
    update dbo.sysdiagrams set definition.write(0x00000C000000070000001C0100000609000062070000480300001A040000DF02, null, 0) where diagram_id = @diagram_id; -- index:5121
    update dbo.sysdiagrams set definition.write(0x0000EC04000027060000B103000027060000CB07000055050000000000000100, null, 0) where diagram_id = @diagram_id; -- index:5153
    update dbo.sysdiagrams set definition.write(0x000088160000506F0000000000002B0000000C00000002000000020000001C01, null, 0) where diagram_id = @diagram_id; -- index:5185
    update dbo.sysdiagrams set definition.write(0x0000F50A00000000000001000000391300009D09000000000000020000000200, null, 0) where diagram_id = @diagram_id; -- index:5217
    update dbo.sysdiagrams set definition.write(0x000002000000020000001C010000150900000100000000000000391300003403, null, 0) where diagram_id = @diagram_id; -- index:5249
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000002000000020000001C010000060900000000, null, 0) where diagram_id = @diagram_id; -- index:5281
    update dbo.sysdiagrams set definition.write(0x000000000000D13100000923000000000000000000000D000000040000000400, null, 0) where diagram_id = @diagram_id; -- index:5313
    update dbo.sysdiagrams set definition.write(0x00001C01000006090000AA0A00009006000078563412040000005A0000000100, null, 0) where diagram_id = @diagram_id; -- index:5345
    update dbo.sysdiagrams set definition.write(0x0000010000000B00000000000000010000000200000003000000040000000500, null, 0) where diagram_id = @diagram_id; -- index:5377
    update dbo.sysdiagrams set definition.write(0x0000060000000700000008000000090000000A00000004000000640062006F00, null, 0) where diagram_id = @diagram_id; -- index:5409
    update dbo.sysdiagrams set definition.write(0x000009000000700061007400690065006E0074007300000002000B00B80B0000, null, 0) where diagram_id = @diagram_id; -- index:5441
    update dbo.sysdiagrams set definition.write(0xACDBFFFF3B020000ACDBFFFF0000000002000000F0F0F0000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5473
    update dbo.sysdiagrams set definition.write(0x000000000000000001000000060000000000000014FEFFFFA5D9FFFFCC110000, null, 0) where diagram_id = @diagram_id; -- index:5505
    update dbo.sysdiagrams set definition.write(0x5801000032000000010000020000CC1100005801000002000000000005000080, null, 0) where diagram_id = @diagram_id; -- index:5537
    update dbo.sysdiagrams set definition.write(0x0800008001000000150001000000900144420100065461686F6D61200066006B, null, 0) where diagram_id = @diagram_id; -- index:5569
    update dbo.sysdiagrams set definition.write(0x005F005F00700061007400690065006E0074005F0069006E0064006900630061, null, 0) where diagram_id = @diagram_id; -- index:5601
    update dbo.sysdiagrams set definition.write(0x0074006F00720073005F005F00700061007400690065006E0074007300214334, null, 0) where diagram_id = @diagram_id; -- index:5633
    update dbo.sysdiagrams set definition.write(0x1208000000391300000E11000078563412070000001401000065007800740065, null, 0) where diagram_id = @diagram_id; -- index:5665
    update dbo.sysdiagrams set definition.write(0x0072006E0061006C005F00690064007300000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5697
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5729
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5761
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5793
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5825
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5857
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:5889
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000000000000000200000005000000540000002C0000, null, 0) where diagram_id = @diagram_id; -- index:5921
    update dbo.sysdiagrams set definition.write(0x002C0000002C0000003400000000000000000000002229000065150000000000, null, 0) where diagram_id = @diagram_id; -- index:5953
    update dbo.sysdiagrams set definition.write(0x002D010000070000000C000000070000001C0100000609000062070000480300, null, 0) where diagram_id = @diagram_id; -- index:5985
    update dbo.sysdiagrams set definition.write(0x001A040000DF020000EC04000027060000B103000027060000CB070000550500, null, 0) where diagram_id = @diagram_id; -- index:6017
    update dbo.sysdiagrams set definition.write(0x000000000001000000881600000E110000000000000500000005000000020000, null, 0) where diagram_id = @diagram_id; -- index:6049
    update dbo.sysdiagrams set definition.write(0x00020000001C010000F50A00000000000001000000391300000E110000000000, null, 0) where diagram_id = @diagram_id; -- index:6081
    update dbo.sysdiagrams set definition.write(0x00050000000500000002000000020000001C0100001509000001000000000000, null, 0) where diagram_id = @diagram_id; -- index:6113
    update dbo.sysdiagrams set definition.write(0x00391300003403000000000000000000000000000002000000020000001C0100, null, 0) where diagram_id = @diagram_id; -- index:6145
    update dbo.sysdiagrams set definition.write(0x00060900000000000000000000D13100000923000000000000000000000D0000, null, 0) where diagram_id = @diagram_id; -- index:6177
    update dbo.sysdiagrams set definition.write(0x0004000000040000001C01000006090000AA0A00009006000078563412040000, null, 0) where diagram_id = @diagram_id; -- index:6209
    update dbo.sysdiagrams set definition.write(0x006200000001000000010000000B000000000000000100000002000000030000, null, 0) where diagram_id = @diagram_id; -- index:6241
    update dbo.sysdiagrams set definition.write(0x000400000005000000060000000700000008000000090000000A000000040000, null, 0) where diagram_id = @diagram_id; -- index:6273
    update dbo.sysdiagrams set definition.write(0x00640062006F0000000D000000650078007400650072006E0061006C005F0069, null, 0) where diagram_id = @diagram_id; -- index:6305
    update dbo.sysdiagrams set definition.write(0x00640073000000214334120800000039130000180C0000785634120700000014, null, 0) where diagram_id = @diagram_id; -- index:6337
    update dbo.sysdiagrams set definition.write(0x01000073006900740065005F0063006F00640065005F00730068006100720065, null, 0) where diagram_id = @diagram_id; -- index:6369
    update dbo.sysdiagrams set definition.write(0x00730000007200650064005F006D0065006400690063006100740069006F006E, null, 0) where diagram_id = @diagram_id; -- index:6401
    update dbo.sysdiagrams set definition.write(0x0073000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6433
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6465
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6497
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6529
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:6561
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000200000005, null, 0) where diagram_id = @diagram_id; -- index:6593
    update dbo.sysdiagrams set definition.write(0x000000540000002C0000002C0000002C00000034000000000000000000000022, null, 0) where diagram_id = @diagram_id; -- index:6625
    update dbo.sysdiagrams set definition.write(0x29000065150000000000002D010000070000000C000000070000001C01000006, null, 0) where diagram_id = @diagram_id; -- index:6657
    update dbo.sysdiagrams set definition.write(0x09000062070000480300001A040000DF020000EC04000027060000B103000027, null, 0) where diagram_id = @diagram_id; -- index:6689
    update dbo.sysdiagrams set definition.write(0x060000CB07000055050000000000000100000088160000930E00000000000004, null, 0) where diagram_id = @diagram_id; -- index:6721
    update dbo.sysdiagrams set definition.write(0x0000000400000002000000020000001C010000F50A0000000000000100000039, null, 0) where diagram_id = @diagram_id; -- index:6753
    update dbo.sysdiagrams set definition.write(0x130000180C000000000000030000000300000002000000020000001C01000015, null, 0) where diagram_id = @diagram_id; -- index:6785
    update dbo.sysdiagrams set definition.write(0x0900000100000000000000391300003403000000000000000000000000000002, null, 0) where diagram_id = @diagram_id; -- index:6817
    update dbo.sysdiagrams set definition.write(0x000000020000001C010000060900000000000000000000D13100000923000000, null, 0) where diagram_id = @diagram_id; -- index:6849
    update dbo.sysdiagrams set definition.write(0x000000000000000D00000004000000040000001C01000006090000AA0A000090, null, 0) where diagram_id = @diagram_id; -- index:6881
    update dbo.sysdiagrams set definition.write(0x06000078563412040000006A00000001000000010000000B0000000000000001, null, 0) where diagram_id = @diagram_id; -- index:6913
    update dbo.sysdiagrams set definition.write(0x0000000200000003000000040000000500000006000000070000000800000009, null, 0) where diagram_id = @diagram_id; -- index:6945
    update dbo.sysdiagrams set definition.write(0x0000000A00000004000000640062006F0000001100000073006900740065005F, null, 0) where diagram_id = @diagram_id; -- index:6977
    update dbo.sysdiagrams set definition.write(0x0063006F00640065005F00730068006100720065007300000021433412080000, null, 0) where diagram_id = @diagram_id; -- index:7009
    update dbo.sysdiagrams set definition.write(0x0039130000220700007856341207000000140100007300690074006500730000, null, 0) where diagram_id = @diagram_id; -- index:7041
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000080000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7073
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7105
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000C611463800090080CC180360F81603, null, 0) where diagram_id = @diagram_id; -- index:7137
    update dbo.sysdiagrams set definition.write(0x60FFFFFFFF00000000000000000000000008BD69150000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7169
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7201
    update dbo.sysdiagrams set definition.write(0x00000000000000F03F0000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7233
    update dbo.sysdiagrams set definition.write(0x000000000000000000D7115138000A0080CC180360F8160360FFFFFFFF000000, null, 0) where diagram_id = @diagram_id; -- index:7265
    update dbo.sysdiagrams set definition.write(0x000000000000000000000000000200000005000000540000002C0000002C0000, null, 0) where diagram_id = @diagram_id; -- index:7297
    update dbo.sysdiagrams set definition.write(0x002C0000003400000000000000000000002229000065150000000000002D0100, null, 0) where diagram_id = @diagram_id; -- index:7329
    update dbo.sysdiagrams set definition.write(0x00070000000C000000070000001C0100000609000062070000480300001A0400, null, 0) where diagram_id = @diagram_id; -- index:7361
    update dbo.sysdiagrams set definition.write(0x00DF020000EC04000027060000B103000027060000CB07000055050000000000, null, 0) where diagram_id = @diagram_id; -- index:7393
    update dbo.sysdiagrams set definition.write(0x000100000088160000180C000000000000030000000300000002000000020000, null, 0) where diagram_id = @diagram_id; -- index:7425
    update dbo.sysdiagrams set definition.write(0x001C010000F50A00000000000001000000391300002207000000000000010000, null, 0) where diagram_id = @diagram_id; -- index:7457
    update dbo.sysdiagrams set definition.write(0x000100000002000000020000001C010000150900000100000000000000391300, null, 0) where diagram_id = @diagram_id; -- index:7489
    update dbo.sysdiagrams set definition.write(0x003403000000000000000000000000000002000000020000001C010000060900, null, 0) where diagram_id = @diagram_id; -- index:7521
    update dbo.sysdiagrams set definition.write(0x000000000000000000D13100000923000000000000000000000D000000040000, null, 0) where diagram_id = @diagram_id; -- index:7553
    update dbo.sysdiagrams set definition.write(0x00040000001C01000006090000AA0A0000900600007856341204000000540000, null, 0) where diagram_id = @diagram_id; -- index:7585
    update dbo.sysdiagrams set definition.write(0x0001000000010000000B00000000000000010000000200000003000000040000, null, 0) where diagram_id = @diagram_id; -- index:7617
    update dbo.sysdiagrams set definition.write(0x0005000000060000000700000008000000090000000A00000004000000640062, null, 0) where diagram_id = @diagram_id; -- index:7649
    update dbo.sysdiagrams set definition.write(0x006F0000000600000073006900740065007300000002000B0072060000F8F4FF, null, 0) where diagram_id = @diagram_id; -- index:7681
    update dbo.sysdiagrams set definition.write(0xFF720600003EFEFFFF0000000002000000F0F0F0000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:7713
    update dbo.sysdiagrams set definition.write(0x0000000000010000001B00000000000000ECF8FFFFEFF8FFFFD70C0000580100, null, 0) where diagram_id = @diagram_id; -- index:7745
    update dbo.sysdiagrams set definition.write(0x0032000000010000020000D70C00005801000002000000000005000080080000, null, 0) where diagram_id = @diagram_id; -- index:7777
    update dbo.sysdiagrams set definition.write(0x8001000000150001000000900144420100065461686F6D61170066006B005F00, null, 0) where diagram_id = @diagram_id; -- index:7809
    update dbo.sysdiagrams set definition.write(0x5F00650078007400650072006E0061006C005F006900640073005F005F007300, null, 0) where diagram_id = @diagram_id; -- index:7841
    update dbo.sysdiagrams set definition.write(0x690074006500730002000B004B1000005AF1FFFFC81900005AF1FFFF00000000, null, 0) where diagram_id = @diagram_id; -- index:7873
    update dbo.sysdiagrams set definition.write(0x02000000F0F0F00000000000000000000000000000000000010000001D000000, null, 0) where diagram_id = @diagram_id; -- index:7905
    update dbo.sysdiagrams set definition.write(0x000000009106000009F2FFFF3918000058010000420000000100000200003918, null, 0) where diagram_id = @diagram_id; -- index:7937
    update dbo.sysdiagrams set definition.write(0x0000580100000200000000000500008008000080010000001500010000009001, null, 0) where diagram_id = @diagram_id; -- index:7969
    update dbo.sysdiagrams set definition.write(0x44420100065461686F6D612A0066006B005F005F0073006900740065005F0063, null, 0) where diagram_id = @diagram_id; -- index:8001
    update dbo.sysdiagrams set definition.write(0x006F00640065005F007300680061007200650073005F005F0073006900740065, null, 0) where diagram_id = @diagram_id; -- index:8033
    update dbo.sysdiagrams set definition.write(0x0073005F005F00730068006100720065005F0073006900740065005F00690064, null, 0) where diagram_id = @diagram_id; -- index:8065
    update dbo.sysdiagrams set definition.write(0x0002000B004B100000F0F1FFFFC8190000F0F1FFFF0000000002000000F0F0F0, null, 0) where diagram_id = @diagram_id; -- index:8097
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000010000001F00000000000000EA0800, null, 0) where diagram_id = @diagram_id; -- index:8129
    update dbo.sysdiagrams set definition.write(0x009FF2FFFF801400005801000043000000010000020000801400005801000002, null, 0) where diagram_id = @diagram_id; -- index:8161
    update dbo.sysdiagrams set definition.write(0x0000000000FFFFFF000800008001000000150001000000900144420100065461, null, 0) where diagram_id = @diagram_id; -- index:8193
    update dbo.sysdiagrams set definition.write(0x686F6D61240066006B005F005F0073006900740065005F0063006F0064006500, null, 0) where diagram_id = @diagram_id; -- index:8225
    update dbo.sysdiagrams set definition.write(0x5F007300680061007200650073005F005F00730069007400650073005F005F00, null, 0) where diagram_id = @diagram_id; -- index:8257
    update dbo.sysdiagrams set definition.write(0x73006900740065005F006900640002000B00A8FDFFFFD6EDFFFFA8FDFFFFE8E1, null, 0) where diagram_id = @diagram_id; -- index:8289
    update dbo.sysdiagrams set definition.write(0xFFFF0000000002000000F0F0F000000000000000000000000000000000000100, null, 0) where diagram_id = @diagram_id; -- index:8321
    update dbo.sysdiagrams set definition.write(0x0000300000000000000057FEFFFF33E7FFFFE20F000058010000320000000100, null, 0) where diagram_id = @diagram_id; -- index:8353
    update dbo.sysdiagrams set definition.write(0x00020000E20F000058010000020000000000FFFFFF0008000080010000001500, null, 0) where diagram_id = @diagram_id; -- index:8385
    update dbo.sysdiagrams set definition.write(0x01000000900144420100065461686F6D611D0066006B005F005F007000610074, null, 0) where diagram_id = @diagram_id; -- index:8417
    update dbo.sysdiagrams set definition.write(0x00690065006E0074005F0069006E00640069006300610074006F00720073005F, null, 0) where diagram_id = @diagram_id; -- index:8449
    update dbo.sysdiagrams set definition.write(0x005F007300690074006500730002000B004E0C0000D6EDFFFF4E0C000099E0FF, null, 0) where diagram_id = @diagram_id; -- index:8481
    update dbo.sysdiagrams set definition.write(0xFF0000000002000000F0F0F00000000000000000000000000000000000010000, null, 0) where diagram_id = @diagram_id; -- index:8513
    update dbo.sysdiagrams set definition.write(0x003200000000000000FD0C00008CE6FFFF960A00005801000032000000010000, null, 0) where diagram_id = @diagram_id; -- index:8545
    update dbo.sysdiagrams set definition.write(0x020000960A000058010000020000000000FFFFFF000800008001000000150001, null, 0) where diagram_id = @diagram_id; -- index:8577
    update dbo.sysdiagrams set definition.write(0x000000900144420100065461686F6D61130066006B005F005F00700061007400, null, 0) where diagram_id = @diagram_id; -- index:8609
    update dbo.sysdiagrams set definition.write(0x690065006E00740073005F005F00730069007400650073000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8641
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8673
    update dbo.sysdiagrams set definition.write(0x0300440064007300530074007200650061006D00000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8705
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8737
    update dbo.sysdiagrams set definition.write(0x160002000300000006000000FFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8769
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000000000000000000001B000000AE0E000000000000, null, 0) where diagram_id = @diagram_id; -- index:8801
    update dbo.sysdiagrams set definition.write(0x53006300680065006D0061002000550044005600200044006500660061007500, null, 0) where diagram_id = @diagram_id; -- index:8833
    update dbo.sysdiagrams set definition.write(0x6C00740000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8865
    update dbo.sysdiagrams set definition.write(0x26000200FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8897
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000560000001600000000000000, null, 0) where diagram_id = @diagram_id; -- index:8929
    update dbo.sysdiagrams set definition.write(0x440053005200450046002D0053004300480045004D0041002D0043004F004E00, null, 0) where diagram_id = @diagram_id; -- index:8961
    update dbo.sysdiagrams set definition.write(0x540045004E005400530000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:8993
    update dbo.sysdiagrams set definition.write(0x2C0002010500000007000000FFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:9025
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000570000007E03000000000000, null, 0) where diagram_id = @diagram_id; -- index:9057
    update dbo.sysdiagrams set definition.write(0x53006300680065006D0061002000550044005600200044006500660061007500, null, 0) where diagram_id = @diagram_id; -- index:9089
    update dbo.sysdiagrams set definition.write(0x6C007400200050006F0073007400200056003600000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:9121
    update dbo.sysdiagrams set definition.write(0x36000200FFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:9153
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000650000001200000000000000, null, 0) where diagram_id = @diagram_id; -- index:9185
    update dbo.sysdiagrams set definition.write(0x650077004D006F00640065003A00310000000100000008001E00000032002C00, null, 0) where diagram_id = @diagram_id; -- index:9217
    update dbo.sysdiagrams set definition.write(0x30002C003200380034002C0030002C0032003800300035000000200000005400, null, 0) where diagram_id = @diagram_id; -- index:9249
    update dbo.sysdiagrams set definition.write(0x610062006C00650056006900650077004D006F00640065003A00320000000100, null, 0) where diagram_id = @diagram_id; -- index:9281
    update dbo.sysdiagrams set definition.write(0x000008001E00000032002C0030002C003200380034002C0030002C0032003300, null, 0) where diagram_id = @diagram_id; -- index:9313
    update dbo.sysdiagrams set definition.write(0x320035000000200000005400610062006C00650056006900650077004D006F00, null, 0) where diagram_id = @diagram_id; -- index:9345
    update dbo.sysdiagrams set definition.write(0x640065003A00330000000100000008001E00000032002C0030002C0032003800, null, 0) where diagram_id = @diagram_id; -- index:9377
    update dbo.sysdiagrams set definition.write(0x34002C0030002C0032003300310030000000200000005400610062006C006500, null, 0) where diagram_id = @diagram_id; -- index:9409
    update dbo.sysdiagrams set definition.write(0x56006900650077004D006F00640065003A00340000000100000008003E000000, null, 0) where diagram_id = @diagram_id; -- index:9441
    update dbo.sysdiagrams set definition.write(0x34002C0030002C003200380034002C0030002C0032003300310030002C003100, null, 0) where diagram_id = @diagram_id; -- index:9473
    update dbo.sysdiagrams set definition.write(0x32002C0032003700330030002C00310031002C00310036003800300000000200, null, 0) where diagram_id = @diagram_id; -- index:9505
    update dbo.sysdiagrams set definition.write(0x00000200000000000000000000000000000000000000D0020000060028000000, null, 0) where diagram_id = @diagram_id; -- index:9537
    update dbo.sysdiagrams set definition.write(0x4100630074006900760065005400610062006C00650056006900650077004D00, null, 0) where diagram_id = @diagram_id; -- index:9569
    update dbo.sysdiagrams set definition.write(0x6F00640065000000010000000800040000003200000020000000540061006200, null, 0) where diagram_id = @diagram_id; -- index:9601
    update dbo.sysdiagrams set definition.write(0x6C00650056006900650077004D006F00640065003A0030000000010000000800, null, 0) where diagram_id = @diagram_id; -- index:9633
    update dbo.sysdiagrams set definition.write(0x3A00000034002C0030002C003200380034002C0030002C003200330031003000, null, 0) where diagram_id = @diagram_id; -- index:9665
    update dbo.sysdiagrams set definition.write(0x2C0031002C0031003800390030002C0035002C00310032003600300000002000, null, 0) where diagram_id = @diagram_id; -- index:9697
    update dbo.sysdiagrams set definition.write(0x00005400610062006C00650056006900650077004D006F00640065003A003100, null, 0) where diagram_id = @diagram_id; -- index:9729
    update dbo.sysdiagrams set definition.write(0x00000100000008001E00000032002C0030002C003200380034002C0030002C00, null, 0) where diagram_id = @diagram_id; -- index:9761
    update dbo.sysdiagrams set definition.write(0x32003800300035000000200000005400610062006C0065005600690065007700, null, 0) where diagram_id = @diagram_id; -- index:9793
    update dbo.sysdiagrams set definition.write(0x4D006F00640065003A00320000000100000008001E00000032002C0030002C00, null, 0) where diagram_id = @diagram_id; -- index:9825
    update dbo.sysdiagrams set definition.write(0x3200380034002C0030002C003200330032003500000020000000540061006200, null, 0) where diagram_id = @diagram_id; -- index:9857
    update dbo.sysdiagrams set definition.write(0x6C00650056006900650077004D006F00640065003A0033000000010000000800, null, 0) where diagram_id = @diagram_id; -- index:9889
    update dbo.sysdiagrams set definition.write(0x1E00000032002C0030002C003200380034002C0030002C003200330031003000, null, 0) where diagram_id = @diagram_id; -- index:9921
    update dbo.sysdiagrams set definition.write(0x0000200000005400610062006C00650056006900650077004D006F0064006500, null, 0) where diagram_id = @diagram_id; -- index:9953
    update dbo.sysdiagrams set definition.write(0x3A00340000000100000008003E00000034002C0030002C003200380034002C00, null, 0) where diagram_id = @diagram_id; -- index:9985
    update dbo.sysdiagrams set definition.write(0x30002C0032003300310030002C00310032002C0032003700330030002C003100, null, 0) where diagram_id = @diagram_id; -- index:10017
    update dbo.sysdiagrams set definition.write(0x31002C00310036003800300000000500000005000000000000005200000001A6, null, 0) where diagram_id = @diagram_id; -- index:10049
    update dbo.sysdiagrams set definition.write(0xDA7B01000000640062006F00000066006B005F005F0070006100740069006500, null, 0) where diagram_id = @diagram_id; -- index:10081
    update dbo.sysdiagrams set definition.write(0x6E0074005F0069006E00640069006300610074006F00720073005F005F007000, null, 0) where diagram_id = @diagram_id; -- index:10113
    update dbo.sysdiagrams set definition.write(0x61007400690065006E007400730000000000000000000000C402000000000600, null, 0) where diagram_id = @diagram_id; -- index:10145
    update dbo.sysdiagrams set definition.write(0x0000060000000500000008000000015FD43C585FD43C0000000000000000AD07, null, 0) where diagram_id = @diagram_id; -- index:10177
    update dbo.sysdiagrams set definition.write(0x00000000000D0000000D00000000000000000000000000000000000000D00200, null, 0) where diagram_id = @diagram_id; -- index:10209
    update dbo.sysdiagrams set definition.write(0x000600280000004100630074006900760065005400610062006C006500560069, null, 0) where diagram_id = @diagram_id; -- index:10241
    update dbo.sysdiagrams set definition.write(0x00650077004D006F006400650000000100000008000400000032000000200000, null, 0) where diagram_id = @diagram_id; -- index:10273
    update dbo.sysdiagrams set definition.write(0x005400610062006C00650056006900650077004D006F00640065003A00300000, null, 0) where diagram_id = @diagram_id; -- index:10305
    update dbo.sysdiagrams set definition.write(0x000100000008003A00000034002C0030002C003200380034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:10337
    update dbo.sysdiagrams set definition.write(0x003300310030002C0031002C0031003800390030002C0035002C003100320036, null, 0) where diagram_id = @diagram_id; -- index:10369
    update dbo.sysdiagrams set definition.write(0x0030000000200000005400610062006C00650056006900650077004D006F0064, null, 0) where diagram_id = @diagram_id; -- index:10401
    update dbo.sysdiagrams set definition.write(0x0065003A00310000000100000008001E00000032002C0030002C003200380034, null, 0) where diagram_id = @diagram_id; -- index:10433
    update dbo.sysdiagrams set definition.write(0x002C0030002C0032003800300035000000200000005400610062006C00650056, null, 0) where diagram_id = @diagram_id; -- index:10465
    update dbo.sysdiagrams set definition.write(0x006900650077004D006F00640065003A00320000000100000008001E00000032, null, 0) where diagram_id = @diagram_id; -- index:10497
    update dbo.sysdiagrams set definition.write(0x002C0030002C003200380034002C0030002C0032003300320035000000200000, null, 0) where diagram_id = @diagram_id; -- index:10529
    update dbo.sysdiagrams set definition.write(0x005400610062006C00650056006900650077004D006F00640065003A00330000, null, 0) where diagram_id = @diagram_id; -- index:10561
    update dbo.sysdiagrams set definition.write(0x000100000008001E00000032002C0030002C003200380034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:10593
    update dbo.sysdiagrams set definition.write(0x003300310030000000200000005400610062006C00650056006900650077004D, null, 0) where diagram_id = @diagram_id; -- index:10625
    update dbo.sysdiagrams set definition.write(0x006F00640065003A00340000000100000008003E00000034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:10657
    update dbo.sysdiagrams set definition.write(0x00380034002C0030002C0032003300310030002C00310032002C003200370033, null, 0) where diagram_id = @diagram_id; -- index:10689
    update dbo.sysdiagrams set definition.write(0x0030002C00310031002C00310036003800300000000E0000000E000000000000, null, 0) where diagram_id = @diagram_id; -- index:10721
    update dbo.sysdiagrams set definition.write(0x00000000000000000000000000D0020000060028000000410063007400690076, null, 0) where diagram_id = @diagram_id; -- index:10753
    update dbo.sysdiagrams set definition.write(0x0065005400610062006C00650056006900650077004D006F0064006500000001, null, 0) where diagram_id = @diagram_id; -- index:10785
    update dbo.sysdiagrams set definition.write(0x00000008000400000032000000200000005400610062006C0065005600690065, null, 0) where diagram_id = @diagram_id; -- index:10817
    update dbo.sysdiagrams set definition.write(0x0077004D006F00640065003A00300000000100000008003A00000034002C0030, null, 0) where diagram_id = @diagram_id; -- index:10849
    update dbo.sysdiagrams set definition.write(0x002C003200380034002C0030002C0032003300310030002C0031002C00310038, null, 0) where diagram_id = @diagram_id; -- index:10881
    update dbo.sysdiagrams set definition.write(0x00390030002C0035002C0031003200360030000000200000005400610062006C, null, 0) where diagram_id = @diagram_id; -- index:10913
    update dbo.sysdiagrams set definition.write(0x00650056006900650077004D006F00640065003A00310000000100000008001E, null, 0) where diagram_id = @diagram_id; -- index:10945
    update dbo.sysdiagrams set definition.write(0x00000032002C0030002C003200380034002C0030002C00320038003000350000, null, 0) where diagram_id = @diagram_id; -- index:10977
    update dbo.sysdiagrams set definition.write(0x00200000005400610062006C00650056006900650077004D006F00640065003A, null, 0) where diagram_id = @diagram_id; -- index:11009
    update dbo.sysdiagrams set definition.write(0x00320000000100000008001E00000032002C0030002C003200380034002C0030, null, 0) where diagram_id = @diagram_id; -- index:11041
    update dbo.sysdiagrams set definition.write(0x002C0032003300320035000000200000005400610062006C0065005600690065, null, 0) where diagram_id = @diagram_id; -- index:11073
    update dbo.sysdiagrams set definition.write(0x0077004D006F00640065003A00330000000100000008001E00000032002C0030, null, 0) where diagram_id = @diagram_id; -- index:11105
    update dbo.sysdiagrams set definition.write(0x002C003200380034002C0030002C003200330031003000000020000000540061, null, 0) where diagram_id = @diagram_id; -- index:11137
    update dbo.sysdiagrams set definition.write(0x0062006C00650056006900650077004D006F00640065003A0034000000010000, null, 0) where diagram_id = @diagram_id; -- index:11169
    update dbo.sysdiagrams set definition.write(0x0008003E00000034002C0030002C003200380034002C0030002C003200330031, null, 0) where diagram_id = @diagram_id; -- index:11201
    update dbo.sysdiagrams set definition.write(0x0030002C00310032002C0032003700330030002C00310031002C003100360038, null, 0) where diagram_id = @diagram_id; -- index:11233
    update dbo.sysdiagrams set definition.write(0x0030000000100000001000000000000000000000000000000000000000D00200, null, 0) where diagram_id = @diagram_id; -- index:11265
    update dbo.sysdiagrams set definition.write(0x000600280000004100630074006900760065005400610062006C006500560069, null, 0) where diagram_id = @diagram_id; -- index:11297
    update dbo.sysdiagrams set definition.write(0x00650077004D006F006400650000000100000008000400000032000000200000, null, 0) where diagram_id = @diagram_id; -- index:11329
    update dbo.sysdiagrams set definition.write(0x005400610062006C00650056006900650077004D006F00640065003A00300000, null, 0) where diagram_id = @diagram_id; -- index:11361
    update dbo.sysdiagrams set definition.write(0x000100000008003A00000034002C0030002C003200380034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:11393
    update dbo.sysdiagrams set definition.write(0x003300310030002C0031002C0031003800390030002C0035002C003100320036, null, 0) where diagram_id = @diagram_id; -- index:11425
    update dbo.sysdiagrams set definition.write(0x0030000000200000005400610062006C00650056006900650077004D006F0064, null, 0) where diagram_id = @diagram_id; -- index:11457
    update dbo.sysdiagrams set definition.write(0x0065003A00310000000100000008001E00000032002C0030002C003200380034, null, 0) where diagram_id = @diagram_id; -- index:11489
    update dbo.sysdiagrams set definition.write(0x002C0030002C0032003800300035000000200000005400610062006C00650056, null, 0) where diagram_id = @diagram_id; -- index:11521
    update dbo.sysdiagrams set definition.write(0x006900650077004D006F00640065003A00320000000100000008001E00000032, null, 0) where diagram_id = @diagram_id; -- index:11553
    update dbo.sysdiagrams set definition.write(0x002C0030002C003200380034002C0030002C0032003300320035000000200000, null, 0) where diagram_id = @diagram_id; -- index:11585
    update dbo.sysdiagrams set definition.write(0x005400610062006C00650056006900650077004D006F00640065003A00330000, null, 0) where diagram_id = @diagram_id; -- index:11617
    update dbo.sysdiagrams set definition.write(0x000100000008001E00000032002C0030002C003200380034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:11649
    update dbo.sysdiagrams set definition.write(0x003300310030000000200000005400610062006C00650056006900650077004D, null, 0) where diagram_id = @diagram_id; -- index:11681
    update dbo.sysdiagrams set definition.write(0x006F00640065003A00340000000100000008003E00000034002C0030002C0032, null, 0) where diagram_id = @diagram_id; -- index:11713
    update dbo.sysdiagrams set definition.write(0x00380034002C0030002C0032003300310030002C00310032002C003200370033, null, 0) where diagram_id = @diagram_id; -- index:11745
    update dbo.sysdiagrams set definition.write(0x0030002C00310031002C00310036003800300000001A0000001A000000000000, null, 0) where diagram_id = @diagram_id; -- index:11777
    update dbo.sysdiagrams set definition.write(0x0040000000014FEA7B01000000640062006F00000066006B005F005F00650078, null, 0) where diagram_id = @diagram_id; -- index:11809
    update dbo.sysdiagrams set definition.write(0x007400650072006E0061006C005F006900640073005F005F0073006900740065, null, 0) where diagram_id = @diagram_id; -- index:11841
    update dbo.sysdiagrams set definition.write(0x00730000000000000000000000C402000000001B0000001B0000001A00000008, null, 0) where diagram_id = @diagram_id; -- index:11873
    update dbo.sysdiagrams set definition.write(0x000000015CD43CD85CD43C0000000000000000AD0700000000001C0000001C00, null, 0) where diagram_id = @diagram_id; -- index:11905
    update dbo.sysdiagrams set definition.write(0x000000000000660000000109680001000000640062006F00000066006B005F00, null, 0) where diagram_id = @diagram_id; -- index:11937
    update dbo.sysdiagrams set definition.write(0x5F0073006900740065005F0063006F00640065005F0073006800610072006500, null, 0) where diagram_id = @diagram_id; -- index:11969
    update dbo.sysdiagrams set definition.write(0x73005F005F00730069007400650073005F005F00730068006100720065005F00, null, 0) where diagram_id = @diagram_id; -- index:12001
    update dbo.sysdiagrams set definition.write(0x73006900740065005F006900640000000000000000000000C402000000001D00, null, 0) where diagram_id = @diagram_id; -- index:12033
    update dbo.sysdiagrams set definition.write(0x00001D0000001C00000008000000015CD43C185CD43C0000000000000000AD07, null, 0) where diagram_id = @diagram_id; -- index:12065
    update dbo.sysdiagrams set definition.write(0x00000000001E0000001E000000000000005A00000001847D5D01000000640062, null, 0) where diagram_id = @diagram_id; -- index:12097
    update dbo.sysdiagrams set definition.write(0x006F00000066006B005F005F0073006900740065005F0063006F00640065005F, null, 0) where diagram_id = @diagram_id; -- index:12129
    update dbo.sysdiagrams set definition.write(0x007300680061007200650073005F005F00730069007400650073005F005F0073, null, 0) where diagram_id = @diagram_id; -- index:12161
    update dbo.sysdiagrams set definition.write(0x006900740065005F006900640000000000000000000000C402000000001F0000, null, 0) where diagram_id = @diagram_id; -- index:12193
    update dbo.sysdiagrams set definition.write(0x001F0000001E000000080000000151D43C9851D43C0000000000000000AD0700, null, 0) where diagram_id = @diagram_id; -- index:12225
    update dbo.sysdiagrams set definition.write(0x000000002F0000002F000000000000004C00000001847D5D0100000064006200, null, 0) where diagram_id = @diagram_id; -- index:12257
    update dbo.sysdiagrams set definition.write(0x6F00000066006B005F005F00700061007400690065006E0074005F0069006E00, null, 0) where diagram_id = @diagram_id; -- index:12289
    update dbo.sysdiagrams set definition.write(0x640069006300610074006F00720073005F005F00730069007400650073000000, null, 0) where diagram_id = @diagram_id; -- index:12321
    update dbo.sysdiagrams set definition.write(0x0000000000000000C4020000000030000000300000002F0000000800000001ED, null, 0) where diagram_id = @diagram_id; -- index:12353
    update dbo.sysdiagrams set definition.write(0x213D88ED213D0000000000000000AD0F00000100003100000031000000000000, null, 0) where diagram_id = @diagram_id; -- index:12385
    update dbo.sysdiagrams set definition.write(0x00380000000100000001000000640062006F00000066006B005F005F00700061, null, 0) where diagram_id = @diagram_id; -- index:12417
    update dbo.sysdiagrams set definition.write(0x007400690065006E00740073005F005F00730069007400650073000000000000, null, 0) where diagram_id = @diagram_id; -- index:12449
    update dbo.sysdiagrams set definition.write(0x0000000000C402000000003200000032000000310000000800000001E9213D48, null, 0) where diagram_id = @diagram_id; -- index:12481
    update dbo.sysdiagrams set definition.write(0xE9213D0000000000000000AD0F00000100001E00000005000000020000000100, null, 0) where diagram_id = @diagram_id; -- index:12513
    update dbo.sysdiagrams set definition.write(0x00004E0000005300000031000000100000000200000032000000010000002F00, null, 0) where diagram_id = @diagram_id; -- index:12545
    update dbo.sysdiagrams set definition.write(0x0000100000000100000000000000310000001E000000100000000E0000004D00, null, 0) where diagram_id = @diagram_id; -- index:12577
    update dbo.sysdiagrams set definition.write(0x0000540000001C000000100000000E0000004B000000520000001A0000001000, null, 0) where diagram_id = @diagram_id; -- index:12609
    update dbo.sysdiagrams set definition.write(0x00000D0000001F0000001E000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:12641
    update dbo.sysdiagrams set definition.write(0x010003000000000000000C0000000B0000004E61BC0000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:12673
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:12705
    update dbo.sysdiagrams set definition.write(0xDBE6B0E91C81D011AD5100A0C90F573900000200D0442758B448D60102020000, null, 0) where diagram_id = @diagram_id; -- index:12737
    update dbo.sysdiagrams set definition.write(0x1048450000000000000000000000000000000000E00100004400610074006100, null, 0) where diagram_id = @diagram_id; -- index:12769
    update dbo.sysdiagrams set definition.write(0x200053006F0075007200630065003D0039003700540047005100560032005C00, null, 0) where diagram_id = @diagram_id; -- index:12801
    update dbo.sysdiagrams set definition.write(0x4D00590032003000310036003B0049006E0069007400690061006C0020004300, null, 0) where diagram_id = @diagram_id; -- index:12833
    update dbo.sysdiagrams set definition.write(0x6100740061006C006F0067003D0065006D00610072003B005000650072007300, null, 0) where diagram_id = @diagram_id; -- index:12865
    update dbo.sysdiagrams set definition.write(0x690073007400200053006500630075007200690074007900200049006E006600, null, 0) where diagram_id = @diagram_id; -- index:12897
    update dbo.sysdiagrams set definition.write(0x6F003D0054007200750065003B0055007300650072002000490044003D007300, null, 0) where diagram_id = @diagram_id; -- index:12929
    update dbo.sysdiagrams set definition.write(0x61003B004D0075006C007400690070006C006500410063007400690076006500, null, 0) where diagram_id = @diagram_id; -- index:12961
    update dbo.sysdiagrams set definition.write(0x52006500730075006C00740053006500740073003D00460061006C0073006500, null, 0) where diagram_id = @diagram_id; -- index:12993
    update dbo.sysdiagrams set definition.write(0x3B0043006F006E006E006500630074002000540069006D0065006F0075007400, null, 0) where diagram_id = @diagram_id; -- index:13025
    update dbo.sysdiagrams set definition.write(0x3D00330030003B00540072007500730074005300650072007600650072004300, null, 0) where diagram_id = @diagram_id; -- index:13057
    update dbo.sysdiagrams set definition.write(0x65007200740069006600690063006100740065003D00460061006C0073006500, null, 0) where diagram_id = @diagram_id; -- index:13089
    update dbo.sysdiagrams set definition.write(0x3B005000610063006B00650074002000530069007A0065003D00340030003900, null, 0) where diagram_id = @diagram_id; -- index:13121
    update dbo.sysdiagrams set definition.write(0x36003B004100700070006C00690063006100740069006F006E0020004E006100, null, 0) where diagram_id = @diagram_id; -- index:13153
    update dbo.sysdiagrams set definition.write(0x6D0065003D0022004D006900630072006F0073006F0066007400200053005100, null, 0) where diagram_id = @diagram_id; -- index:13185
    update dbo.sysdiagrams set definition.write(0x4C00200053006500720076006500720020004D0061006E006100670065006D00, null, 0) where diagram_id = @diagram_id; -- index:13217
    update dbo.sysdiagrams set definition.write(0x65006E0074002000530074007500640069006F00220000000080050012000000, null, 0) where diagram_id = @diagram_id; -- index:13249
    update dbo.sysdiagrams set definition.write(0x500061007400690065006E00740073000000000226000C000000730069007400, null, 0) where diagram_id = @diagram_id; -- index:13281
    update dbo.sysdiagrams set definition.write(0x65007300000008000000640062006F0000000002260022000000730069007400, null, 0) where diagram_id = @diagram_id; -- index:13313
    update dbo.sysdiagrams set definition.write(0x65005F0063006F00640065005F00730068006100720065007300000008000000, null, 0) where diagram_id = @diagram_id; -- index:13345
    update dbo.sysdiagrams set definition.write(0x640062006F000000000226001A000000650078007400650072006E0061006C00, null, 0) where diagram_id = @diagram_id; -- index:13377
    update dbo.sysdiagrams set definition.write(0x5F00690064007300000008000000640062006F00000000022600120000007000, null, 0) where diagram_id = @diagram_id; -- index:13409
    update dbo.sysdiagrams set definition.write(0x61007400690065006E0074007300000008000000640062006F00000000022400, null, 0) where diagram_id = @diagram_id; -- index:13441
    update dbo.sysdiagrams set definition.write(0x26000000700061007400690065006E0074005F0069006E006400690063006100, null, 0) where diagram_id = @diagram_id; -- index:13473
    update dbo.sysdiagrams set definition.write(0x74006F0072007300000008000000640062006F00000001000000D68509B3BB6B, null, 0) where diagram_id = @diagram_id; -- index:13505
    update dbo.sysdiagrams set definition.write(0xF2459AB8371664F0327008004E0000007B003100360033003400430044004400, null, 0) where diagram_id = @diagram_id; -- index:13537
    update dbo.sysdiagrams set definition.write(0x37002D0030003800380038002D0034003200450033002D003900460041003200, null, 0) where diagram_id = @diagram_id; -- index:13569
    update dbo.sysdiagrams set definition.write(0x2D004200360044003300320035003600330042003900310044007D0000000000, null, 0) where diagram_id = @diagram_id; -- index:13601
    update dbo.sysdiagrams set definition.write(0x010003000000000000000C0000000B0000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:13633
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:13665
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:13697
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:13729
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:13761
    update dbo.sysdiagrams set definition.write(0x0000000000000000000000000000000000000000000000000000000000000000, null, 0) where diagram_id = @diagram_id; -- index:13793
    update dbo.sysdiagrams set definition.write(0x62885214, null, 0) where diagram_id = @diagram_id; -- index:13825

    print '=== Diagram [Patients] restored at diagram_id=' + cast(@diagram_id as varchar(10)) + '. ===';
end try
begin catch
    delete from dbo.sysdiagrams where diagram_id = @diagram_id;
    print '=== ' + error_message() + ' ===';
end catch;
-- End of restore diagram [Patients] script.
end;
