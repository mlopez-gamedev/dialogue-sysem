﻿using MiguelGameDev.DialogueSystem.Commands;
using MiguelGameDev.DialogueSystem.Parser.Command;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

namespace MiguelGameDev.DialogueSystem.Editor
{
    public class HighlightCommentParser : CommandParser
    {
        public override string StartsWith => "//";
        private readonly IHighlightCommandFactory _highlightCommandFactory;

        private readonly string _startWithColor;
        private readonly string _commentColor;

        public HighlightCommentParser(IHighlightCommandFactory highlightCommandFactory, HighlightStyle style)
        {
            _highlightCommandFactory = highlightCommandFactory;
            _startWithColor = "#" + ColorUtility.ToHtmlStringRGB(style.CommentStartColor);
            _commentColor = "#" + ColorUtility.ToHtmlStringRGB(style.CommentColor);
        }

        protected override bool TryParse(string lineCommand, CommandPath commandPath, out IDialogueCommand command)
        {
            if (!lineCommand.StartsWith(StartsWith))
            {
                command = null;
                return false;
            }

            var highlightedText = GetBranchStarts(commandPath.Level);
            highlightedText += HighlightText(lineCommand);
            command = _highlightCommandFactory.CreateHighlightCommand(highlightedText);
            return true;
        }

        private string HighlightText(string lineCommand)
        {
            string highlightedCommand = $"<color={_startWithColor}>{StartsWith}</color>";

            lineCommand = Regex.Unescape(lineCommand.Substring(StartsWith.Length));
            highlightedCommand += $"<color={_commentColor}>{lineCommand}</color>";
            return highlightedCommand;
        }
    }

}
