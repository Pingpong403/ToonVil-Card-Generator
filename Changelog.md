# v1.0 Generator released

## v1.1
- added option to repeat generation process immediately after completion
- removed comment in Cards.txt that suggested manually bolding and italicizing, which are scrapped features
- prevented error when trying to make a card for a deck that's missing the card template
- changed keyword colors to their intended colors
- added clarification for Villain names in Keywords.txt
- more visually centered the cost value
- fixed Gains Action Symbol field to allow an underscore
- updated the tutorial to be more helpful

## v1.2
- element positioning is now fully configurable
- line spacing and word distance are now configurable
- the generator no longer handles drawing the icon

# v2.0 Text drawing improvements
- refactored text measuring
- when an Activated Ability is below text, it will go to the right side
- introduced the -Settings folder

## v2.1
- added padding between abilities
- when Activate Cost follows the "Pay x Power(.)" format, it will be bolded; colon is now centered between symbol and text
- Ability area is now shorter
- added error handling for missing -Settings files
- added error handling for missing elements
- when finding the right font size, the generator now makes smaller increments; the increment size can now be configured
- fixed bug causing end-of-ability newlines to not count towards total text height
- made a slight improvement when using "%" vs " % "
- included new typeMaxWidth setting
- min/max settings are now configurable (does nothing at the moment)
- slight tutorial improvement