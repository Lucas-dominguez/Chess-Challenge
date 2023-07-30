# The Scout Bot
By Lucas DOMINGUEZ

## Description
Use algo **Negascout** (variant of negamax/minamax) with :
- Board evalution is done with pieces values, and piece square values (encoded in long) (only mid game for now)
- Transposition table
- Move sort scoring according to
  - Transposition move
  - Capture move
  - Killer move

## Result

Optimal depth -> 6 (1s by move)

## Todo
Encode late game square values with midgame, to keep in token limit
