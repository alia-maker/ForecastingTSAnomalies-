   M S B A M L   `   `   `  ����P  LTimeSeriesForecasting, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:TimeSeriesForecasting.ViewsTimeSeriesForecasting.Views  5   	   S.clr-namespace:TimeSeriesForecasting.ViewModels TimeSeriesForecasting.ViewModels  5   	   ;    5TimeSeriesForecasting.Views.ModelParamsSelectorWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   ;local)clr-namespace:TimeSeriesForecasting.Views     5   	   =vm.clr-namespace:TimeSeriesForecasting.ViewModels     5   	   -   5        $��SelectorWindow��5	   	     � Title$  Выбор модели��5
   	   $	��450��6   $	��700��6+    � DataContext 5      <   6TimeSeriesForecasting.ViewModels.ModelParamsSelectorVM  5   
   .��5      �  � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6    � RowDefinitions 5   
   �� 5      $	��10* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
   �� $��Center=�6    �� Orientation$ 
Horizontal=�60   $��2��6I   $��1��6]   .+�5   
   �� $2��,Коэффициент отклонения:��6   $��Center=�63   $��0��6U   �� 5   
   $��35��61   $��22��6<   $��Center=�6J   ��6   �� *ScalingFactor+6J   5      �� 5!   
   $A��;Отображать результат в браузере��6   $��7,0,0,0q�6=   $��Center=�6N   $��2��5"      $��4��6    $��14��64    d� 	IsChecked 6@   �� *PlotBrawser+�� 5$   
    �� 	IsDefault . $��Выбрать��6"   
�������$��3��6L   $��Center=�6Z   $��0q�5%      $��0��6   $��90��6.   ��5&      �� *DetectAnomaly+��5'      ��  �� ElementName$ SelectorWindow���� 5(   
    . $��Отмена��6"   
�������$��3��6K   $��Center=�6Y   $��0q�5)      $��1��6   $��90��6.   ��5*      �� *CloseWindow+��5+      �� $ SelectorWindow��&   System.Windows.Controls.DataGrid  5,   
   $��2��6   $��0��5-         AutoGenerateColumns . ��5.      �� *ModelParamsSource+	 �� SelectedItem	 50      �� *ModelParamData+52      53      