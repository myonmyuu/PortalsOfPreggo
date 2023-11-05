# PortalsOfPreggo
A pregnancy expansion for Portals of Phereon.

# Content
- Female/Futa characters have menstrual cycles
- Characters' wombs can have multipe Ovums/Eggs
- All (pretty much all) sexual activity involving both a penis and a vagina will cause sperm transfer into the recipient
- the contents (ovums, eggs, sperm) of a character's uterus is displayer via a small window

  it can be expanded to show the name and species of the donor
- eggs are formed in the womb after a successful impregnation

  they take time to grow, and are added to the egg inventory after birth

## Farm/Hatching
eggs keep track of who the father/mother is (barring brothel customers and other transient characters).
Characters can refer to you as Mom/Dad if you are one of the parents (configurable).
Farm pairs insemniate each other instead of directly creating eggs.
 
## Items
A small assortment of items to interact with pregnancy (can be disabled if users feel that they dilute the item pool too much).
All items affect a single character.

### Common 
- Weak Pregnancy Accelerator
  - Progresses all active pregnancies by 30%
- Ovulatory Stimulant
  - Progresses the menstural cycle (~3 charges needed to get from start of Menstruation to Ovulation)
### Rare
- Strong Pregnancy Accelerator
  - Progresses all active pregnancies by 70%
- Egg Transmitter
  - "Moves fertilized/implanted eggs to a nest in a different world." I don't think I need to elaborate what that means
- Birth Control Potion
  - Enables/Disables ovulation
### Legendary
- Pregnancy Finalizer
  - Immediately completes all active pregnancies
- Fertility Cure
  - Removes the Infertile trait from a character (or adds Fertile)
- Ovulation Potion
  - Immediately causes target to ovulate

Male/Futa characters also have a new option in their character screen, "Collect Cum". For 1 energy, attain a cum potion for that character. Using this potion on a character inserts a load of cum into them from the relevant character.
Due to how the game saves items, these cum potions *do not* persist when saving & loading, so make sure to use all uses or sell them before quitting.

## Brothel
Characters working at a brothel (show rooms exempt) will have cum added from random customers as days pass.

## Dates
The player character and date partner can cum inside each other when the appropriate cards are played.
Vaginally penetrating actions will take virginities (configurable)

## Events
When certain events trigger, cum can be added to involved characters. These events are triggerable through Lua. One example is featured (Sylvie & Castalia portal event).

# Balancing
Pregnancies take time, and chances are that it'll be one egg at a time. Taking this into consideration, birthed eggs begin with hatching progress based on pregnancy time and have extra stability. Additionally, random genetics of the egg will be increased based on the parent's virility and fertility. (configs for this are ToDo)
In an effort to make the main character remain a valid breeding partner, their genetics increase when they birth eggs with better genetics. (toggable in config)
