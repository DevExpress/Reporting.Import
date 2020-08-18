@echo off
SET TMPDIR=%TMP%
C:\DX\InternalSoftware\GrammarTools\jay.exe -c ExpressionGrammar.y <C:\DX\InternalSoftware\GrammarTools\skeleton.cs >ExpressionGrammar.cs