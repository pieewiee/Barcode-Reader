   M S B A M L   `   `   `  ���� I  EBarcode Reader, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:QR_Code_ReaderQR_Code_Reader      barcode_Reader.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     .localclr-namespace:QR_Code_Reader     -     �Name$  Canvas�� � Title$ Barcode Reader��$��507.254��$��643.586�� � 
ResizeMode$ NoResize=�.��� -    �Name$ mainbox�� J� Opacity$	 0.8Q�.+��� -    ��Name$ pictureBox1��$
��Left=�$	��480��$	��Top=�$	��640��$��	-4,-4,0,0q��� -   $ image��$��Right=�$��63��$��	0,10,10,0q�$	��Top=�$	��176���� -    ��Name$ toggleCam_Copy��   MetroButton#���   $��0,0,602,442q�$��24��$��Bottom=�$��Right=�$��24��$�� ��$��16(�$��Impact)� J� RenderTransformOrigin$ 0.994,0.459&���Z�  J� OpacityMask �� 	 �� EndPoint$	 0.5,1&�
 �� 
StartPoint$
 0.5,0&�.���  � Color$ Black�� � Offset$ 0Q�� $ White��$ 1Q�����  �� ImageSource$ Resources\Gear.png���� -    ��Name$ GroupBoxSettings��$�� ��$
��Left=�$	��342��$��	19,41,0,0q�$	��Top=�$	��594��
���   �
�������.���� -    ��Name$ StackPanelMainSettings��$
��Left=�$	��316��$��
20,0,-12,0q�$	��Top=�$	��574��.+��� -   $ StackPanelDebug��$
��Left=�$��63��$��0q�$	��Top=�$	��520��.+��� -	   $ StackPanelSliders��$
��Left=�$��30��$��0,0,0,0q�$	��Top=�$	��563�� �� Orientation$ 
Horizontal=�.+��� -
    ��Name$ labelFPS��$
��FPS:��$
��Left=�$��25��$	��Top=��� -    ��Name$ sliderFrames��$
��Left=�$	��Top=�$	��100�� � Maximum$ 25Q� � SmallChange$ 1Q� �� TickPlacement$ TopLeft=� �� IsSnapToTickEnabled . �� -   $ labelThread��$��Timeout m/s:��$
��Left=�$	��Top=��� -   $ sliderThread��$
��Left=�$	��Top=�$	��100��$ 10000Q�$ 1Q� .  �� TickFrequency$	 100Q�$ TopLeft=� � Minimum$ 1Q��� -   $ labelFreeze��$��Freeze Frames:��$
��Left=�$	��Top=��� -   $ sliderFreeze��$
��Left=�$	��Top=�$	��100��$	 300Q�$ 1Q� . $ 10Q�$ TopLeft=�$ 1Q��� $
��Left=�$��25��$��0,0,0,0q�$	��Top=�$"��520                         ��$ 
Horizontal=�.+��� -   $ LabelSliderFramesMin��$��25��$
��Left=�$	��Top=�$��	25,0,70,0q��� -   $ LabelSliderFramesMax��$��0��$
��Left=�$	��Top=�$��0,0,75,0q��� -   $ LabelSliderThreadMin��$��1��$
��Left=�$	��Top=�$��0,0,70,0q��� -   $ LabelSliderThreadMax��$��0��$
��Left=�$	��Top=�$��0,0,85,0q��� -   $ LabelSliderFreezeMin��$��1��$
��Left=�$	��Top=�$��0,0,70,0q��� -   $ LabelSliderFreezeMax��$��0��$
��Left=�$	��Top=�$��0q��� $
��Left=�$��55��$��0q�$	��Top=�$	��594��.+��� $
��Left=�$��28��$��0,0,0,0q�$	��Top=�$	��594��$ 
Horizontal=�.+��� -   $ label1��$��New Domain:��$
��Left=�$	��Top=�$	��129��$��28��$��	0,0,217,0q��� -    ��Name$ checkBoxPrefix��$��Enable Prefix��$
��Left=�$	��Top=�$��0,7,33,0q��� -   $ button1��$��Delete��$
��Left=�$��0,2,0,0q�$	��Top=�$��75���� $
��Left=�$��24��$��0,0,0,0q�$	��Top=�$	��594��$ 
Horizontal=�.+��� -    ��Name$ textBox��$
��Left=�$��23��$��0,0,10,0q� �� TextWrapping$
 Wrap=�$	��Top=�$	��327���� -   $ textBoxPrefix��$
��Left=�$��23��$
 Wrap=�$	��Top=�$	��120��$ 0.519,1.565&�$��0,0,10,0q��� -   $ button��$	��Add��$
��Left=�$	��Top=�$��75��$ 	7.36,5.55&�$��0,1,0,0q��� $
��Left=�$	��169��$��0q�$	��Top=�$	��594��.+��� -   $ label��$��
Whitelist:��$
��Left=�$	��Top=�$	��129��$��0q�&   System.Windows.Controls.DataGrid  -     Name$ dataGrid��$
��Left=�$	��143��$	��Top=�$	��541��$��0q�   Columns 0  *System.Windows.Controls.DataGridTextColumn  ,  &System.Windows.Controls.DataGridColumn   Header$ Domain��1  +System.Windows.Controls.DataGridBoundColumn   Binding �� *Domain+  $ Prefix�� �� *Prefix+�� $
��Left=�$��27��$��0,5,0,0q�$	��Top=�$	��594��$ 
Horizontal=�.+��� -   $ AimCheckBox��$	��Aim��$
��Left=�$��19��$��0q�$	��Top=��� -   $ SoundcheckBox��$��Sound��$
��Left=�$��19��$��	5,0,366,0q�$	��Top=��� -    $ ImageFolderButton��$��Image Folder��$
��Left=�$��0,0,17,0q�$	��Top=�$��75���� -!     ��Name$  	Statusbar��$
��Left=�$��21��$��	0,455,0,0q�$	��Top=�$	��636��.@��� -"   $ 
consoleBox��$��Content:��$	��626��$	��Top=�$��	0,-10,0,0q��� -#   ! ��Name$! 	Crosshair��
���   �$	 0.5Q�$��202,120,202,122q����� $ Resources\pattern.png���� -$   $ 	Autopaste��#���   $��0,0,567,442q�$��24��$��Bottom=�$��Right=�$��24��$�� ��$��16(�$��Impact)�$ 0.994,0.459&���Z� ����  �� $	 0.5,1&�$
 0.5,0&�.��� $ Black��$ 0Q�� $ White��$ 1Q��� -%   $ label2��$��	Autopaste��$��Right=�$��0,0,455,421q�$��Bottom=����	#FF998F8F$��20(�$	��112��$��53���� -&   $ StackPanelMainButtons��$
��Left=�$��51��$��143,397,0,0q�$	��Top=�$	��353��$ 
Horizontal=�.+��� -'   $ 
exitButton��#���   $��0,0,10,0q�$��49��$	��Top=�$
��Left=�$��78��$
��EXIT��$	 0.8Q�$��36(�$��Impact)����	#FF1A347F���	#00000000$ -0.339,0.556&��� -(   $ resetButton_Copy1��#���   $��0,0,10,0q�$��49��$	��Top=�$
��Left=�$��78��$��RESET��$	 0.8Q�$��25(�$��Impact)����	#FF1A347F���	#00000000$ -0.339,0.556&��� -)   $ SaveImage_Copy��#���   $��0,0,10,0q�$��49��$	��Top=�$
��Left=�$��78��$��
Save
Image��$	 0.8Q�$��16(�$��Impact)����	#FF1A347F���	#00000000$ -0.339,0.556&��� -*   $ 	toggleCam��#���   $��0,0,10,0q�$��49��$	��Top=�$
��Left=�$��78��$��Toggle 
Camera��$	 0.8Q�$��16(�$��Impact)����	#FF1A347F���	#00000000