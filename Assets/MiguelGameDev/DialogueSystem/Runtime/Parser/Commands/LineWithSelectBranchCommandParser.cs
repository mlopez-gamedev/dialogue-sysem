﻿using MiguelGameDev.DialogueSystem.Commands;
using System.Linq;
using System.Text.RegularExpressions;

namespace MiguelGameDev.DialogueSystem.Parser.Command
{
    public class LineWithSelectBranchCommandParser : CommandParser
    {
        public override string StartsWith => "- ";
        public const string AuthorSeparatorPattern = @"(?<!\\): ";
        public const string SelectionSplitter = "\n* ";
        public const string BranchSplitter = "\n\t";
        private readonly char[] MessageTrim = new char[] { ' ', '\n' };

        private readonly ILineWithSelectBranchCommandFactory _lineWithSelectBranchCommandFactory;
        private readonly BranchParser _branchParser;

        public LineWithSelectBranchCommandParser(ILineWithSelectBranchCommandFactory lineCommandFactory, BranchParser branchParser)
        {
            _lineWithSelectBranchCommandFactory = lineCommandFactory;
            _branchParser = branchParser;
        }

        protected override bool TryParse(string lineCommand, CommandPath commandPath, out IDialogueCommand command)
        {
            if (!lineCommand.StartsWith(StartsWith))
            {
                command = null;
                return false;
            }

            UnityEngine.Debug.Log($"lineCommand: {lineCommand}");
            var lineAndBranches = lineCommand.Split(SelectionSplitter, System.StringSplitOptions.RemoveEmptyEntries);

            if (lineAndBranches.Length < 1)
            {
                command = null;
                return false;
            }
            UnityEngine.Debug.Log($"lineAndBranches.Length: {lineAndBranches.Length}");
            var line = CreateLine(lineAndBranches[0].Substring(StartsWith.Length));

            var selectBranchInfos = new SelectBranchInfo[lineAndBranches.Length - 1];
            for (int i = 1; i < lineAndBranches.Length; ++i)
            {
                var branchIndex = i - 1;
                selectBranchInfos[branchIndex] = CreateSelectBranchInfo(commandPath, branchIndex, lineAndBranches[i]);
            }

            command = _lineWithSelectBranchCommandFactory.CreateLineWithSelectBranchCommand(line, selectBranchInfos);
            return true;
        }

        private Line CreateLine(string lineCommand)
        {
            var match = Regex.Match(lineCommand, AuthorSeparatorPattern);

            if (!match.Success)
            {
                return new Line(lineCommand);
            }


            var author = Regex.Unescape(lineCommand.Substring(0, match.Index));
            var message = Regex.Unescape(lineCommand.Substring(match.Index + match.Length).Trim(MessageTrim));

            return new Line(author, message);
        }

        private SelectBranchInfo CreateSelectBranchInfo(CommandPath commandPath, int branchIndex, string lineCommand)
        {
            var splits = lineCommand.Split(BranchSplitter);

            if (splits.Length > 1)
            {
                var branchPosition = new BranchPosition(commandPath.CommandIndex, branchIndex);
                var branchText = lineCommand.Substring(splits[0].Length);
                var branch = _branchParser.Parse(branchText, commandPath.CommandIndex, commandPath.Level + 1, commandPath.BranchPositions.Append(branchPosition).ToArray());

                return new SelectBranchInfo(splits[0], branchPosition, branch);
            }

            return new SelectBranchInfo(splits[0]);
        }
    }
}
