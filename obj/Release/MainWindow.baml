   M S B A M L   `   `   `  ���� R  NadcLogeris_WPFFramework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullA%clr-namespace:adcLogeris_WPFFrameworkadcLogeris_WPFFramework  U QLiveCharts.Wpf, Version=0.9.7.0, Culture=neutral, PublicKeyToken=0bc1f845d1ebb8dfG4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.WpfLiveCharts.Wpf (    "adcLogeris_WPFFramework.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     7local%clr-namespace:adcLogeris_WPFFramework     Dlvc4clr-namespace:LiveCharts.Wpf;assembly=LiveCharts.Wpf     -     � Title$  Data logger��$��707.833��$��765.167��.��� .+��� -   $��75��$��	10,10,0,0q�$
��Left=�$��
Scan ports��$��22��$	��Top=��� -    ��Name$ COMPort_ComboBox��$	��120��$��	90,10,0,0q�$
��Left=�$	��Top=��� -   $	��120��$��22��$��	90,37,0,0q�$
��Left=�$	��Top=�$��
Disconnect���� $��61��$��61��$��
238,10,0,0q�$
��Left=�$	��Top=����� *mStatusImage+ �� UpdateSourceTrigger$ PropertyChanged=���  � Cursor$ Arrow��$��4��$��61��$��
305,10,0,0q�$
��Left=�$	��Top=��� -    �� TextWrapping$ NoWrap=� �� TextAlignment$
 Left=�$��366,10,40,0q�$��22��$	��Top=� �� 
IsReadOnly . ���� *	mFilePath+�� $��Path:��$
��Left=�$��	314,6,0,0q�$	��Top=��� $��Name:��$
��Left=�$��
314,37,0,0q�$	��Top=��� -   $��failoVardas��$
 Left=�$��22��$��366,41,10,0q�$ NoWrap=�$	��Top=�����  �� Path$ 	mFileName�$ PropertyChanged=� �� Mode$ TwoWay=� �� *
mIrasineti+�� -   $	��...��$��Right=�$��	0,10,10,0q�$	��Top=�$��25��$��22���� -   $��Start��$��	10,0,0,10q�$��30��$��Bottom=�$
��Left=�$��75���� -	   $��statusLabel��$��Not recording��$��
90,0,10,10q�$��30��$��Bottom=��� $ Arrow��$��4��$��
10,76,10,0q�$��Stretch=�$	��Top=��� $��Period (300 - 60000)��$
��Left=�$��
314,85,0,0q�$	��Top=�$
��Left=�$��Center=��� -
   $	��Set��$
��Left=�$��424,116,0,0q�$	��Top=�$��40��$��22���� -   $��periodoLaukas��$
��Left=�$��22��$��314,116,0,0q�$ NoWrap=�$	��Top=�$��75��$
��1000��$
��Left=�$��Center=��� $ Arrow��$��4��$��
10,0,10,45q�$��Stretch=�$��Bottom=��� $ Arrow��$��4��$��10,178,10,0q�$��Stretch=�$	��Top=��� $ Arrow��$��4��$��88��$��
305,85,0,0q�$
��Left=�$	��Top=��� $��10,187,10,54q�.@��� $
��Main��.��� ���	#FFE5E5E5.+�� $��84,79,83,98q�$��Center=�$	��564��.+��� $��ADC values (V):��$
��Left=�$��	10,10,0,0q�$	��Top=��� $ Center=�$
��Left=�$��20��$��	10,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[0]+�� $��1��$
��Left=�$��	10,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��	65,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[1]+�� $��2��$
��Left=�$��	65,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
120,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[2]+�� $��3��$
��Left=�$��
120,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
175,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[3]+�� $��4��$
��Left=�$��
175,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
230,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[4]+�� $��5��$
��Left=�$��
230,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
285,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[5]+�� $��6��$
��Left=�$��
285,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
340,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[6]+�� $��7��$
��Left=�$��
340,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
395,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[7]+�� $��8��$
��Left=�$��
395,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
450,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[8]+�� $��9��$
��Left=�$��
450,36,0,0q�$	��Top=�$��50��$��Center=��� $ Center=�$
��Left=�$��20��$��
505,67,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[9]+�� $��10��$
��Left=�$��
505,36,0,0q�$	��Top=�$��50��$��Center=��� $��Temperature:��$
��Left=�$��	10,92,0,0q�$	��Top=��� $ Center=�$
��Left=�$��20��$��
118,98,0,0q�$
 Wrap=�$	��Top=�$��50�� . ���� *mKanalaiVertes[10]+�� $��Last distance:��$
��Left=�$��
10,123,0,0q�$	��Top=���  . $
��Left=�$��23��$��118,126,0,0q�$ NoWrap=�$	��Top=�$��86������ *mLastDistance+�� $��Last amplitude:��$
��Left=�$��
10,154,0,0q�$	��Top=���  . $
��Left=�$��23��$��118,157,0,0q�$ NoWrap=�$	��Top=�$��86������ *mLastAmplitude+�� $��mm��$
��Left=�$��209,123,0,0q�$	��Top=��� $��mm��$
��Left=�$��209,154,0,0q�$	��Top=��� $	��ºC��$
��Left=�$��
173,92,0,0q�$	��Top=��� -   	 ��Name$	 lastWrittenLine��$��
10,0,10,10q� . $ NoWrap=�$��23��$��Bottom=��� $��Last written line��$
��Left=�$��	10,0,0,38q�$��26��$��Bottom=��� $��Chamber��.��� ���	#FFE5E5E5.+��� -   $��75��$��	10,10,0,0q�$
��Left=�$��
Scan ports��$��22��$	��Top=��� -   $ COMPort_ComboBox_Krosnis��$��86��$��	90,10,0,0q�$
��Left=�$	��Top=��� -   $!��atsijungimoMygtukas_krosnis��{�.  $��86��$��22��$��	90,37,0,0q�$
��Left=�$	��Top=�$��
Disconnect���� $
��Left=�$��23��$��	10,37,0,0q�$	��Top=�$��23������ *mKrosnisStatus+�� -   $
��Left=�$��23��$��	10,95,0,0q�$
 Wrap=�$	��Top=�$��72��{��� *mKrosnisNePaleista+���� *mKrosniesPradinisTaskas+$ TwoWay=��� -   $
��Left=�$��23��$��
104,95,0,0q�$
 Wrap=�$	��Top=�$��72��{��� *mKrosnisNePaleista+���� *mKrosniesStatumas+$ TwoWay=��� $��Start point��$
��Left=�$��	10,64,0,0q�$	��Top=�$��65���� $��Slope (0-99)��$
��Left=�$��
104,64,0,0q�$	��Top=�� $��
198,65,0,0q����	#FFBBBBBB.+�#  LiveCharts.Wpf.CartesianChart  $��
10,10,10,0q�&   LiveCharts.Wpf.Charts.Base.Chart
   DisableAnimations
 .    	Hoverable .    DataTooltip Z�    Series   LiveCharts.Wpf.LineSeries    LiveCharts.Wpf.Series   Fill �	#FF2D7C00   LineSmoothness$ 1Q�   Stroke �	#FF004B07   StrokeThickness$ 6Q�   PointGeometrySize$ 17Q�   Values �� *EinamasTaskasValue+  
 ���� $ 1Q� �#F34336$ 3Q� �� *ChartValues+   AxisX   LiveCharts.Wpf.Axis     
Foreground
 �   �   Title$
 Time��   LabelsRotation$	 -45Q�   LabelFormatter �� *DateTimeFormatter+   MaxValue �� *	AxisMax+   MinValue �� *	AxisMin+   Unit �� *
AxisUnit+   	Separator   LiveCharts.Wpf.Separator     Step �� *
AxisStep+   AxisY   
 �   �$ Temperature (°C)���� -   $��TempKeitimoIntervalas��$
��Left=�$��23��$��
10,262,0,0q�$
 Wrap=�$	��Top=�$	��166��$��1��{��� *mKrosnisNePaleista+�� $"��Temp. change interval (min.)��$
��Left=�$��
10,231,0,0q�$	��Top=��� $��Temperature (-40 to 99)��$
��Left=�$��	198,6,0,0q�$	��Top=��� -   $��NextTempValue��$
��Left=�$��23��$��
198,37,0,0q�$
 Wrap=�$	��Top=�$��99��$��24��{��� *mKrosnisNePaleista+�� -   $	��Add��$
��Left=�$��
302,37,0,0q�$	��Top=�$��41��$��23��{��� *mKrosnisNePaleista+�� $��Preset functions��$
��Left=�$��	415,6,0,0q�$	��Top=��� -   $��funkcijosForma��$
��Left=�$��
415,37,0,0q�$	��Top=�$��91�� �� SelectedIndex$ 0��$��23��{��� *mKrosnisNePaleista+.@��� .��Sine�� .��Cosine�� .��Line�� -   $��TaskuSkaiciusFunkcijoj��$
��Left=�$��22��$��
511,37,0,0q�$
 Wrap=�$	��Top=�$��57��$��20��{��� *mKrosnisNePaleista+�� $��Points��$
��Left=�$��	511,6,0,0q�$	��Top=�$��57���� -   $��Generate��$
��Left=�$��
573,37,0,0q�$	��Top=�$��57��$��22��{��� *mKrosnisNePaleista+�� -   $��START��$
��Left=�$��	10,0,0,10q�$	��166��$��43��$��Bottom=��� -   $��Reset function��$
��Left=�$��
10,290,0,0q�$	��Top=�$	��166��{��� *mKrosnisNePaleista+� $
��Left=�$��26��$��
635,10,0,0q�$	��Top=�$��86��.+��� $��Right=�$��28��$
��Left=����� *ChartValues.Count+�� $
��Left=�$��28��$��Right=����� *mTemperaturosSkaitliukas+�� $	��+1/��$
��Left=�$��28,0,0,0q�$	��Top=�$��30��	�   	� 	GroupName$  TempKeitimoVeiksmai��$:��4Wait for correct SENSOR
temperature before changing��$
��Left=�$��
10,135,0,0q�$	��Top=�! d� 	IsChecked$
! TrueY�{��� *mKrosnisNePaleista+	� $  TempKeitimoVeiksmai��$;��5Wait for correct CHAMBER
temperature before changing��$
��Left=�$��
10,171,0,0q�$	��Top=�$��32��$	��183��{��� *mKrosnisNePaleista+! �� *KrosnisTempWait+�� -   $��krosnisTimeStamp��$�� ��$��635,37,10,0q�$	��Top=�$��Right=��� -   $��10(�$��Center=�$��Center=�$
��Left=�$��17��$��
22,211,0,0q�$ NoWrap=�$	��Top=�$��20������ *
mTempRibos+$ TwoWay=�{��� *mKrosnisNePaleista+�� $��+-��$
��Left=�$��	0,205,0,0q�$	��Top=�$��26��$��22���� $��ms��$
��Left=�$��390,116,0,0q�$	��Top=�$��25���� -   $
��Left=�$��	10,85,0,0q�$	��Top=�$��88��$��88��$��Connect���� $��Sensor found��$
��Left=�$��
116,85,0,0q�$	��Top=��� $��Configuration server��$
��Left=�$��116,116,0,0q�$	��Top=��� $��Measurement server��$
��Left=�$��116,147,0,0q�$	��Top=��� $
��Left=�$��26��$��26��$��253,116,0,0q�$	��Top=����� *mConfServerStatus+$ PropertyChanged=��� $
��Left=�$��26��$��26��$��253,147,0,0q�$	��Top=����� *mMeasServerStatus+$ PropertyChanged=��� $
��Left=�$��26��$��26��$��
253,85,0,0q�$	��Top=����� *mSensorStatus+$ PropertyChanged=��� $��Points per file��$
��Left=�$��
578,85,0,0q�$	��Top=��� -   $��TaskuSkaiciusFaile��$
��Left=�$��23��$��578,116,0,0q�$
 Wrap=�$	��Top=�$��82��$��50000���� $��/��$
��Left=�$��557,113,0,0q�$	��Top=�$��16���� $��Right=�$
��Left=�$��482,113,0,0q�$	��Top=�$��70��$��26������ *mEiluciuSkaitliukas+