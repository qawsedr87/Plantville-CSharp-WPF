   M S B A M L   `   `   `  ����I  ERLinPlantville, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null/clr-namespace:RLinPlantvilleRLinPlantville  5   	       RLinPlantville.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   .localclr-namespace:RLinPlantville     5   	   -   5          � Title$  
MainWindow��5	   	   $	��450��6   $	��560��6)   .��5
      � .+�5   
   �� -   5         ��Name$ seed_btn��5      $��Seeds Emporium��6#   $
��Left=�6<   $��
85,164,0,0q�6W   $	��Top=�6k   $	��118��6�   6�   �� 5   
   -   5        $ 
garden_btn��5      $��Garden��6%   $
��Left=�66   $��
85,100,0,0q�6Q   $	��Top=�6e   $	��118��6}   6�   �� 5   
   -   5        $ inventory_btn��5      $��	Inventory��6(   $
��Left=�6<   $��
85,132,0,0q�6W   $	��Top=�6k   $	��118��6�   6�   �� 5   
   -   5         ��Name$ title_tb��5      $
��Left=�6&   $��
240,88,0,0q�6A   $
��Wrap=�6U   $��Garden��6i   $	��Top=�6w   $��20(�6�   $
��Bold"�6�   �� 5   
   -   5        $ subtitle_tb��5      $
��Left=�6)   $��240,119,0,0q�6D   $
��Wrap=�6Y   $%��What you would like to harvest?��6m   $	��Top=�6�   �� 5   
   -   5         ��Name$ 	garden_lb��5      $
��Left=�6%   $	��180��6@   $��240,142,0,0q�6M   $	��Top=�6b   $	��230��6z   6�   �� 5   
   -   5        $ inventory_lb��5      $
��Left=�6(   $	��180��6C   $��240,142,0,0q�6P   $	��Top=�6e   $	��230��6}   $y�	Collapsed=�6�   �� 5   
   -	   5        $ seed_lb��5      $
��Left=�6#   $	��180��6>   $��240,142,0,0q�6K   $	��Top=�6`   $	��230��6x   $y�	Collapsed=�6�   6�   �� 5   
   -
   5        $ money_tb��5      $
��Left=�6&   $��
85,306,0,0q�6A   $
��Wrap=�6U   $��Money��6i   $	��Top=�6v   �� 5   
   -   5        $ land_tb��5      $
��Left=�6%   $��
85,327,0,0q�6@   $
��Wrap=�6T   $
��Land��6h   $	��Top=�6t   �� 5   
   -   5        $ garden_harvest_btn��5      $��Harvest all��6-   $
��Left=�6C   $��396,338,0,0q�6^   $	��Top=�6s   $��74��6�   6�   �� 5   
   -   5        $ inventory_sell_btn��5      $��Sell in farmer's market��6-   $
��Left=�6O   $��320,338,0,0q�6j   $	��Top=�6   $	��150��6�   $y�	Collapsed=�6�   6�   �� 5   
   $
��Left=�6   $��
76,196,0,0q�6/   $
��Wrap=�6C   $	��Top=�6W   $��10(�6o   �� 6~   $%�name  seed / harvest / duration��6�   �� 6�   �� 6�   $#�strawberries  $2 / $8 / 30sec��6�   �� 6�   �� 6�   $�spinach  $5 / $21 / 1min��6�   �� 6  �� 6   $�pears  $2 / $20 / 3mins��6$  6F  �� 5   
   $
��Left=�6   $��
150,34,0,0q�6/   $
��Wrap=�6C   $	��Top=�6W   $��10(�6o   �� 6~   $D�>1. Spoiled plant can be harvested but can't store in inventory��6�   �� 6�   �� 6�   $5�/2. While plant is growing, you couldn't harvest��6�   �� 6  �� 6  $@�:3. spoiled time = harvest time + harvest duration + 15mins��6#  �� 6g  �� 6s  $=�74. harvest left time < 1min, status would be "harvest" ��6w  6�  5      5      