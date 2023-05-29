# Plantville: Cheat sheet

This game works our ability to deconstruct large projects. If you get stuck, this cheat sheet will help you break down the project into manageable pieces. Attached are rough examples of what each phase should look like. 

## Phase 1
1. Lay out user interface. This step is easy and lets us see what we are building fast. A great way to create communication between the app and what we want to see.

2. Create grids. Each button (e.g. Garden, Seeds Emporium) shows a different list box, title, etc. This is a great example of when to use grids.
3. Clicking the button shows different grids. Create the button event handlers that displays the different grids.
When you click Garden, you see the Garden title and listbox. 
When you click Seeds Emporium, you see the Seeds Emporium title and listbox.
4. Create seed inventory list. Add this to your code-behind. We're going to need it. This will control what seeds are on sale in the Emporium.

```csharp
List seed_list = new List() {
     new Seed(*TODO*),
     new Seed(*TODO*),
     new Seed(*TODO*)
};
```

5. Create Seed class. What information do we need for a Seeds class? If you don't know, start with what you know. "Name" is useful for one.
6. Loading ListBoxes. When you click the Seed Emporium button, it should display the available seeds.
Click on Gardens should show empty listbox.
Click on Seeds Emporium should show list of seeds for sale.
7. Conclusion: We should have a UI rough draft with grids and two list boxes. We should be able to control what seeds are on sale by changing `seed_list`.


## Phase 2
1. Create money system. Players should start with $100 and be able to purchase seeds. They can buy seeds when they have enough money.
2. Create land plot system. Cannot exceed their land plots. Verify players have enough money and land. 
3. Purchase confirmation. When players purchase seeds (double-click) in Seed Emporium, give confirmation in message box. If they can't afford it, let them know why.

## Phase 3
1. Display garden. When players buy seeds, list it in the garden.
2. Time to harvest. Display how much time to harvest in garden

## Phase 4
1. Harvest all button. When button is clicked, removes all plants ready for harvest from list.
2. ...


Hopefully this should get your ball rolling enough that you could complete the rest on your own. Have fun!