namespace Mesch.Jyro

open FParsec.Primitives
open FParsec.CharParsers

/// Helper module for position tracking in the parser
module PositionTracking =
    /// Create a Position from FParsec position
    let positionFromFParsec (pos: FParsec.Position) : Position =
        { Line = int pos.Line
          Column = int pos.Column
          StartIndex = int pos.Index
          EndIndex = int pos.Index }

    /// Create a Position with start and end from two FParsec positions
    let positionFromRange (startPos: FParsec.Position) (endPos: FParsec.Position) : Position =
        { Line = int startPos.Line
          Column = int startPos.Column
          StartIndex = int startPos.Index
          EndIndex = int endPos.Index }

    /// Get the current position in a parser
    let getPos<'u> : Parser<Position, 'u> =
        getPosition |>> positionFromFParsec

    /// Wrap a parser to capture its position range
    let withPos<'a, 'u> (p: Parser<'a, 'u>) : Parser<'a * Position, 'u> =
        pipe3 getPosition p getPosition (fun s v e -> (v, positionFromRange s e))
