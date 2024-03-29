O12(SHL SCREW P0130)
$1
(<MACHINEID>L22</MACHINEID>)
(P0130 SHOULDER SCREWS)
(SIZES P0130.050 AND ABOVE ONLY)
(FOR SIZES P0130.040 USE PROG 67)
(FOR P0130.030 USE PROG 1201-WOBBLE BROACH)


#511=6.0(D2=#511)
#512=8.0(D1=#512)
#513=11.85(D3=#513)
#514=11.00(L2=#514)
#515=10.0(L1=#515)
#516=5.90(H=#516)
#517=4.0(A/F=#517)
#518=0.02(L1 TOL)
#519=0.0125(D1 TOL)


(ADD-THIS-LINE-TO-OTHER-PRG)
(IN ORDER TO RUN SCHEDULE PROG)
IF[#17GT#0]THEN#515=#560


#520=[#515+#518]
#521=[#512-#519]

#522=4.0(BORE SIZE)
#523=4.075(A/F SIZE)
#524=2500(FEEDRATE/MIN)
#525=3.7(HEXAGON DEPTH)
#526=0.05(DEPTH OF CUT)
#527=6.0(NUMBER OF BROACHES)

(RPM VARIABLE FOR ALL VARIANTS)
(SETS RPM ON D1 AND HEAD TURNING AND ALSO D1 UNDERCUT)
(RPM VARIABLE IS #540)

IF[#512EQ5.0]THEN#540=3000
IF[#512EQ6.0]THEN#540=2700
IF[#512EQ8.0]THEN#540=2500
IF[#512EQ10.0]THEN#540=1800
IF[#512EQ12.0]THEN#540=1500

!L1

IF[#599EQ1]GOTO99

(START UP)


#555=[#514+#520+#516](OAL)

M9
M52
G50Z[#500-#501]-#502
M6
M350Z10000
M360X3Y3Z3
G13.1G18G97G99G113
M89M94M96M124
G0X#814+1.0Z-1.0

(CHECKS BAR HAS GONE)
M51

G630(MAIN/SUB SIMULTANEOUS)

(USES T5 FOR ALL TURNING)


(FACE + TURN FOR THREAD)
(HOLDER SDJCR1212-E11S)
(INSERT VCET)
M3S1=2000T500
G0X#814+1.0Z0T5
G1X-1.0F0.03
G0Z-1.0


IF[#511EQ4.0]GOTO340
IF[#511EQ6.0]GOTO300
IF[#511EQ8.0]GOTO400
IF[#511EQ10.0]GOTO401

(M5 THREAD)
S1=3000
G1G41X-0.6Z0
X#511,C0.85F0.03
Z[#514-2.0]
X3.77A-30.0F0.02
Z#514,R0.5
X#814,C0.1F0.03
W0.5
G0G40X#814+1.0T0
GOTO5


N340(M4 THREAD)
G165P1Q1.5D1.1
S1=3000G1G41X-0.8Z0
X#511,C0.7F0.03
Z[#514-1.9]
X2.83A-30.0F0.02
Z#514
X#814,C0.1F0.03
W0.5
G165P0
G0G40X#814+1.0T0
GOTO5

N300(M6 THREAD)

S1=3000
G1G41X-0.6Z0
X#511,C1.0F0.03
Z[#514-2.5]
X4.49A-30.0F0.02
Z#514,R0.55
X#814,C0.2F0.03
W0.5
G0G40X#814+1.0T0
GOTO5


N401(M10 THREAD)
S1=1732
G165P1Q2.0D1.5
G1G41X-0.6Z0
X#511,C1.5F0.03
Z[#514-3.7]
X7.75A-30.0F0.02
Z#514,R0.6
X#814,C0.2F0.02
W0.5
G165P0
G0G40X#814+1.0T0
GOTO5


N400(M8 THREAD)
S1=1732
G165P1Q2.0D1.5
G1G41X-0.2Z0
X#511,C1.2F0.03
Z[#514-3.1]
X6.14A-30.0F0.03
Z#514,R0.64
X#814,C0.1F0.03
W0.5
G165P0
G0G40X#814+1.0T0
N5

(SCREWCUT D2)
M3S1=1500T200
G0X#814+1.0Z-1.0T2
G76P020060Q0.02R0.02


IF[#511EQ4.0]GOTO540
IF[#511EQ6.0]GOTO500
IF[#511EQ8.0]GOTO600
IF[#511EQ10.0]GOTO610

(M5 THREAD)
X6.0
G76X4.019Z#514-0.1P0.491Q0.2F0.8
GOTO6


N540(M4 THREAD)
X5.0
G76X3.141Z#514-0.25P0.429Q0.1F0.7
GOTO6

N500(M6 THREAD)
X7.0
G76X4.773Z#514-0.3P0.613Q0.3F1.0
GOTO6

N610(M10 THREAD)
X11.0
G76X8.16Z#514-0.1P0.920Q0.3F1.5
GOTO6

N600(M8 THREAD)
X9.0
G76X6.466Z#514-0.1P0.767Q0.2F1.25
N6G0X#814+1.0T0

(RE CHAMFER THREAD TO DEBURR)
M3S1=2000T500
G0X#814+1.0Z1.5T5


IF[#511EQ4.0]GOTO343
IF[#511EQ6.0]GOTO303
IF[#511EQ8.0]GOTO404
IF[#511EQ10.0]GOTO414

(M5 THREAD)
X6.0

G1G42X#511F0.05
Z0,C0.85F0.03
X-1.0F0.05
Z-1.0
G0G40X6.0
Z[#514-2.0]-0.5
G1G41X#511
Z[#514-2.0]
X4.77A-30.0
W0.5
G0G40X#814+1.0T0
GOTO7


N343(M4 THREAD)
S1=3000
X5.0
G1G42X#511F0.05
Z0,C0.7F0.03
X-1.0F0.04
Z-1.0
G0G40X7.0
Z[#514-2.5]-0.5
G1G41X#511
Z[#514-1.9]
X2.83A-30.0
W0.5
G0G40X#814+1.0T0
GOTO7

N303(M6 THREAD)
X7.0
G1G42X#511F0.05
Z0,C1.0F0.03
X-1.0F0.05
Z-1.0
G0G40X7.0
Z[#514-2.5]-0.5
G1G41X#511
Z[#514-2.5]
X4.49A-30.0
W0.3
G0G40X#814+1.0T0
GOTO7

N414(M10 THREAD)
X11.0
G1G42X#511F0.05
Z0,C1.5F0.03
X-1.0F0.05
Z-1.0
G0G40X11.0
Z[#514-4.5]
G1G41X#511
Z[#514-3.7]
X7.75A-30.0
W0.5
G0G40X#814+1.0T0
GOTO7


N404(M8 THREAD)
X9.0
G1G42X#511F0.05
Z0,C1.2F0.03
X-1.0F0.05
Z-1.0
G0G40X9.0
Z[#514-3.1]-0.5
G1G41X#511
Z[#514-3.1]
X6.14A-30.0
W0.5
G0G40X#814+1.0T0
N7

(RE SCREWCUT TO DEBURR)
M3S1=1500T200
G0X#814+1.0Z-1.0T2
G76P000060Q0.02R0.02

IF[#511EQ4.0]GOTO404
IF[#511EQ6.0]GOTO505
IF[#511EQ8.0]GOTO606
IF[#511EQ10.0]GOTO616


(M5 THREAD)
X6.0
G76X4.019Z#514-0.1P0.491Q0.491F0.8
GOTO8

N404(M4 THREAD)
X5.0
G76X3.141Z#514-0.25P0.429Q0.2F0.7
GOTO8

N505(M6 THREAD)
X7.0
G76X4.773Z#514-0.3P0.613Q0.613F1.0
GOTO8

N616(M10 THREAD)
X11.0
G76X8.16Z#514-0.1P0.920Q0.920F1.5
GOTO8

N606(M8 THREAD)
X9.0
G76X6.466Z#514-0.1P0.767Q0.767F1.25
N8G0X#814+1.0T0

(TURN D1)
(VCET)
M3S1=#540T500
G0X#814+1.0Z[#514-1.0]T5
G1G41X#521-1.0
G165P1Q1.5D0.75
G1Z#514-0.2F0.03
X#521,C0.4F0.03
Z[#514+#520]-0.1,R.6F.03
X#513,C0.25F.03
Z#555+1.0F.03
X#814
W0.5
G165P0
G0G40X#814+1.0T0

(U/CUT D1)
(VCET)
M3S1=#540T500
G0X#814+1.0Z[#514+#520]T5

IF[#512EQ4.0]GOTO704
IF[#512EQ5.0]GOTO705
IF[#512EQ8.0]GOTO700
IF[#512EQ10.0]GOTO800
IF[#512EQ12.0]GOTO810

(D1 = 6MM)
G165P1Q1.5D1.5
G1X#521-0.15,R0.2F0.02
W-0.7
X#521A45.0
X#521+0.025W0.5F0.03
G165P0
G0X#814+1.0T0
GOTO9

N704(D1 = 4MM)
S1=3000
G165P1Q1.5D1.5
G1X#521-0.3,R0.1F0.02
W-0.7
X#521A45.0
X#521+0.025W0.5F0.03
G165P0
G0X#814+1.0T0
GOTO9

N705(D1 = 5MM)
S1=3000
G165P1Q1.5D1.5
G1X#521-0.3,R0.05F0.02
W-0.7
X#521A45.0
X#521+0.025W0.5F0.03
G165P0
G0X#814+1.0T0
GOTO9

N700(D1 = 8MM)
G165P1Q1.5D1.5
G1X#521-0.3,R0.2F0.02
W-1.0
X#521A45.0
X#521+0.025W0.5F0.03
G165P0
G0X#814+1.0T0
GOTO9

N810(D1 = 12MM)
G165P1Q1.5D1.5
G1X#521-0.3,R0.6F0.02
W-1.0
X#521A45.0
X#521+0.025W0.5F0.03
G165P0
G0X#814+1.0T0
GOTO9


N800(D1 = 10MM)
G165P1Q1.5D1.5
G1X#521-0.3,R0.4F0.02
W-1.0
X#521A45.0
X#521+0.025W0.5F0.03
G165P0
G0X#814+1.0T0
GOTO9

N9

(POSITION BAR FOR CUT-OFF)
M3S1=#817T100
G0X#814+1.0Z#555+#500+#502T1

G650

(SYNC FOR RPM + ANGLE)
G114.1H1D-2R0
M88
!2L1


M88

(CUT-OFF)
G1X#824F#822

(CANCELS RPM SYNC)
G113

(CHECKS IF CUT-OFF BROKE)
M3S1=300M25
G4U1.0
M50

G600
M89

(CHECKS BARFEED)
M8
/M98P666666
M9

(RETURN TO ST.POS)
M5
M7
G4U1.2
G0X824Z[#500-#501]-#502T0
G4U0.1
M6

(COUNTS)
M56

N99(LAST PART)
G999
G630
N999
M53

M2
M99
$2
!L1

(START UP)
M360X3Z3
T3000

IF[#599EQ1]GOTO99(SUB SP. ONLY)

G630
IF[#598EQ1]GOTO1(MAIN SP. ONLY)
M98H31(FACE P.PT)
M98H32(SPOT P.PT)
M98H34(DRILL P.PT)
M98H33(BROACH P.PT)
M98H321(RE CHAMFER P.PT)
M98H341(RE DRILL P.PT)
N1
M98P3000(EJECT)

G650W0

(RAPID POSITION SUB SPINDLE)
M24S2=#817
M72
G0Z-1.0

(CONFIRMS RPM SYNC)
M77

(PICK OFF)
G1G98Z#555-#821F5000
G4U0.1
M15
M88

!1L1
M73

G600
M89

N99(LAST PART)
G999
G630
M98H31(FACE L.PT)
M98H32(SPOT L.PT)
M98H34(DRILL L.PT)
M98H33(BROACH L.PT)
M98H321(CHAMFER L.PT)
M98H341(RE DRILL L.PT)
M53
M98P3000(EJECT)
N999

M2
M99

N34(SUB FOR DRILLING)
G44G99
M88
M23S2=2000T3400Q3
G0Z-1.0T34

IF[#517EQ2.5]GOTO802
IF[#517EQ4.0]GOTO80
IF[#517EQ5.0]GOTO81
IF[#517EQ6.0]GOTO811

(3MM HEX)
M23S2=3164
G74R0.1
G74Z4.0Q0.5F0.04
G0G80Z-1.0
GOTO82


N802(2.5MM HEX)
M23S2=3164
G74R0.1
G74Z3.4Q0.5F0.04
G0G80Z-1.0
GOTO82


N80(4MM HEX)
M23S2=2000
G74R0.1
G74Z5.0Q0.2F0.04
G0G80Z-1.0
GOTO82


N81(5MM HEX)
M23S2=1732
G74R0.1
G74Z6.0Q0.5F0.04
G0G80Z-1.0
GOTO82


N811(6MM HEX)
M23S2=1500
G74R0.1
G74Z6.9Q0.5F0.04
G0G80Z-1.0
GOTO82

N82G0Z-1.0T0
M99

N341(SUB FOR RE DRILLING)
G44G99
M88
M23S2=2000T3400Q3
G0Z-1.0T34
Z0

IF[#517EQ2.5]GOTO852
IF[#517EQ4.0]GOTO85
IF[#517EQ5.0]GOTO86
IF[#517EQ6.0]GOTO816

(3MM HEX)
M23S2=3164
G1Z4.0F0.03
G0G80Z-1.0
GOTO87



N852(2.5MM HEX)
M23S2=3164
G1Z3.4F0.04
GOTO87


N85(4MM HEX)
M23S2=2000
G1Z5.0F0.02
G0G80Z-1.0
GOTO87

N816(6MM HEX)
M23S2=1500
G1Z6.9F0.025
G0G80Z-1.0
GOTO87


N86(5MM HEX)
M23S2=1732
G1Z6.0F0.025

N87G0Z-1.0T0
M99

N32(SUB FOR SPOT)
(8MM X 90' SPOTTER)
G44G99
M88
M23S2=3000T3200Q3
G0Z-1.0T32


IF[#517EQ2.5]GOTO552
IF[#517EQ4.0]GOTO55
IF[#517EQ5.0]GOTO56
IF[#517EQ6.0]GOTO516

(3MM HEX)
G1Z1.8F0.02
GOTO50


N552(2.5MM HEX)
G1Z1.58F0.02
GOTO50


N55(4MM HEX)
G1Z2.25F0.02
GOTO50

N516(6MM HEX)
S2=1500
G1Z3.5F0.02
G4U0.1
GOTO50

N56(5MM HEX)
S2=1800
G1Z2.90F0.02
G4U0.1
N50
G0Z-1.0T0
M99

N321(SUB FOR CHAMFER)
G44G99
M88
M23S2=3000T3200Q3
G0Z-1.0T32
Z0


IF[#517EQ2.5]GOTO572
IF[#517EQ4.0]GOTO57
IF[#517EQ5.0]GOTO58
IF[#517EQ6.0]GOTO581

(3MM HEX)
G1Z1.8F0.02
GOTO60



N572(2.5MM HEX)
G1Z1.58F0.02
GOTO60

N57(4MM HEX)
G1Z2.25F0.02
GOTO60

N581(6MM HEX)
S2=1500
G1Z3.5F0.02
G4U0.1

N58(5MM HEX)
G1Z2.90F0.02
N60G4U0.1
G0Z-1.0T0
M99

N31(SUB FOR FACING)
G44G99
M88
M23S2=2500T3100Q3
G0X#814+1.0Z-1.0T31
Z1.0
G1G42X#513F0.03
Z0,C0.5
X-1.0
Z-1.0
G0G40X#814+1.0T0M25
M99

N33(SUB ROUTINE FOR BROACH)
(2MM-5MM A/F)
(HORN SHAVING)
M25G98T3300Q3E0
G0X#522Z-1.0T33

M350Z4000(SLOWS Z2 RAPID DOWN)

#528=#523*1.1547(DO NOT CHANGE)
#529=0(DO NOT CHANGE)
#530=[[#528-#522]/#526](NO. OF)
(PASSES)

#531=0(D0 NOT CHANGE)

WHILE[#529LT#527]DO1
WHILE[#531LT#530]DO2

G0U#526
G1Z#525F#524
G4U0.1
G1U-#526
G0Z-2.0
U#526

#531=[#531+1]
END2


#531=0(RESET DEPTH COUNTER)

G0X#522H[360.0/#527]Z-2.0

#529=[#529+1.0]
END1

G0Z-2.0

M351(FULL Z RAPID)

G0Z-2.0T0(CANCELS OFFSETS)
M99











$0
A0-LE-2-20-8-P-M
#814=0000012000
#815=0000002000
#816=0000001000
#817=0002000000
#822=0000000050
#824=-000001000
#818=0000065000
#819=0000001000
#918=0000000000
#821=0000007000
#921=0000000000
#919=0000000000
#922=0000000000
#990=0004101000
#991=0000051000
#992=0000060000
#893=0000000000
#25974=0004050000
#25975=0004058000
#25976=0004058000
#25977=0004058000
#25978=0000000000
#25979=0000000000
#25980=0000000000
%