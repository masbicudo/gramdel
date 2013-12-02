using System;
using System.Collections.Generic;
using System.Text;

namespace Gramdel.Core
{
    public class GramdelParser
    {
        public void Gramdel(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Gramdel);

            var rules = new List<RuleNode>();

            // waiting for the first rule
            var prodRule = context.GetOrCreateProduction(this.Rule);
            SuccessContinuation<RuleNode> ruleProcessor = null;
            ruleProcessor = (key, ruleNode, ctx) =>
                {
                    // If there are no more rules, then we signal the production of a new Gramdel file.
                    if (ruleNode == null)
                    {
                        prod.ItemProduced(0, ctx, new GramdelNode { Rules = rules.ToArray() });
                        prod.CloseProduction(ctx);
                    }
                    else
                    {
                        rules.Add(ruleNode);

                        // waiting for another rule
                        var prodRule2 = ctx.GetOrCreateProduction(this.Rule);
                        prodRule2.ContinueWith(ruleProcessor);
                        prodRule2.Execute(ctx);
                    }
                };
            prodRule.ContinueWith(ruleProcessor);
            prodRule.Execute(context);
        }

        private void Alternation(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Alternation);

            context.Position = prod.Origin;

            // Sets the minimum number of alternatives that will be analysed by this producer method.
            prod.SetAlternativeProductsCapacity(2);

            var operands = new List<ExpressionNode>();
            var productionConcat = context.GetOrCreateProduction(this.Concatenation);
            FailureContinuation fail = ctx =>
                {
                    prod.ProductionFailed(0, ctx);
                    prod.CloseProduction(ctx);
                };
            productionConcat.FailWith(fail);
            SuccessContinuation<ExpressionNode> cont1 = null;
            cont1 = (key, node, ctx) =>
                {
                    operands.Add(node);

                    if (operands.Count >= 1)
                        prod.CreateAlternativeSlots(0, 1);

                    if (operands.Count > 1)
                        prod.ItemProduced(1, ctx, new AlternationNode { Operands = operands.ToArray() });
                    else if (operands.Count == 1)
                        prod.ItemProduced(1, ctx, node);

                    ctx.Position += ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);
                    if (ctx.Reader.TryReadToken(ctx.Code, ctx.Position, "|"))
                    {
                        ctx.Position++;
                        ctx.Position += ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);

                        // reading the second operator of the alternation
                        var productionConcat2 = context.GetOrCreateProduction(this.Concatenation);
                        productionConcat2.FailWith(fail);
                        productionConcat2.ContinueWith(cont1);
                        productionConcat2.Execute(ctx);
                    }
                };
            productionConcat.ContinueWith(cont1);
            productionConcat.Execute(context);
        }

        private void Concatenation(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Alternation);

            context.Position = prod.Origin;

            // Sets the minimum number of alternatives that will be analysed by this producer method.
            prod.SetAlternativeProductsCapacity(1);

            var operands = new List<ExpressionNode>();
            var productionConcat = context.GetOrCreateProduction(this.ExpressionTerm);
            FailureContinuation fail = ctx =>
                {
                    prod.ProductionFailed(0, ctx);
                    prod.CloseProduction(ctx);
                };
            productionConcat.FailWith(fail);
            SuccessContinuation<ExpressionNode> cont1 = null;
            cont1 = (key, node, ctx) =>
                {
                    operands.Add(node);

                    prod.CreateAlternativeSlots(0, 1);
                    if (operands.Count > 1)
                        prod.ItemProduced(1, ctx, new AlternationNode { Operands = operands.ToArray() });
                    else if (operands.Count == 1)
                        prod.ItemProduced(1, ctx, node);

                    // spaces are concatenation operators in this case
                    var spaces = ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);
                    if (spaces > 0)
                    {
                        ctx.Position += spaces;

                        // reading the second operator of the alternation
                        var productionConcat2 = context.GetOrCreateProduction(this.ExpressionTerm);
                        productionConcat2.FailWith(fail);
                        productionConcat2.ContinueWith(cont1);
                        productionConcat2.Execute(ctx);
                    }
                };
            productionConcat.ContinueWith(cont1);
            productionConcat.Execute(context);
        }

        private void ExpressionTerm(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.ExpressionTerm);

            prod.SetAlternativeProductsCapacity(2);
            const int ALT_TOKEN = 0;
            const int ALT_NAME = 1;

            // trying to read a token
            {
                bool isAltTokenFailed = false;

                context.Position = prod.Origin;

                if (context.Reader.TryReadToken(context.Code, context.Position, "'"))
                {
                    context.Position++;

                    var b = new StringBuilder();
                    var tokenData = context.Reader.ReadText(context.Code, context.Position, c => c != '\'');
                    context.Position += tokenData.Length;
                    b.Append(tokenData);
                    while (context.Reader.TryReadToken(context.Code, context.Position, "''"))
                    {
                        context.Position += 2;
                        b.Append('\'');
                        tokenData = context.Reader.ReadText(context.Code, context.Position, c => c != '\'');
                        context.Position += tokenData.Length;
                        b.Append(tokenData);
                    }

                    if (context.Reader.TryReadToken(context.Code, context.Position, "'"))
                    {
                        context.Position++;
                        prod.ItemProduced(ALT_TOKEN, context, new TokenNode { Token = b.ToString() });
                    }
                    else
                        isAltTokenFailed = true;
                }
                else
                    isAltTokenFailed = true;

                if (isAltTokenFailed)
                    prod.ProductionFailed(ALT_TOKEN, context);
            }

            // trying to read a name
            {
                context.Position = prod.Origin;

                var name = context.Reader.ReadText(context.Code, context.Position, c => char.IsLetterOrDigit(c));
                if (name.Length > 0)
                {
                    context.Position += name.Length;
                    prod.ItemProduced(ALT_NAME, context, new RuleReferenceNode { RuleName = name });
                }
                else
                {
                    prod.ProductionFailed(ALT_NAME, context);
                }
            }
        }

        public void Rule(ParsingLocalContext context)
        {
            var prod = context.GetOrCreateProduction(this.Rule);

            var ruleNode = new RuleNode();

            context.Position += context.Reader.SkipSpaces(context.Code, context.Position);
            if (context.Reader.TryReadToken(context.Code, context.Position, "rule"))
            {
                context.Position += 4;
                var spaces = context.Reader.SkipSpaces(context.Code, context.Position);

                if (spaces > 0)
                {
                    context.Position += spaces;
                    ruleNode.Name = context.Reader.ReadText(context.Code, context.Position, c => char.IsLetterOrDigit(c));
                    context.Position += ruleNode.Name.Length;

                    context.Position += context.Reader.SkipSpaces(context.Code, context.Position);

                    if (context.Reader.TryReadToken(context.Code, context.Position, "->"))
                    {
                        context.Position += 2;
                        context.Position += context.Reader.SkipSpaces(context.Code, context.Position);

                        var prodAlt = context.GetOrCreateProduction(this.Alternation);
                        prodAlt.ContinueWith<ExpressionNode>((key, expressionNode, ctx) =>
                            {
                                ctx.Position += ctx.Reader.SkipSpaces(ctx.Code, ctx.Position);

                                if (ctx.Reader.TryReadToken(ctx.Code, ctx.Position, ";"))
                                {
                                    ctx.Position++;
                                    ruleNode.Expression = expressionNode;
                                    prod.ItemProduced(0, context, ruleNode);
                                }
                            });
                        prodAlt.Execute(context);
                    }
                }
            }
        }

        public class GramdelNode
        {
            public RuleNode[] Rules { get; set; }

            public GramdelNode Clone()
            {
                return new GramdelNode
                {
                    Rules = CloningUtility.CloneArray(this.Rules),
                };
            }
        }

        public class RuleNode : ICloneable<RuleNode>
        {
            public string Name { get; set; }
            public ExpressionNode Expression { get; set; }

            public RuleNode Clone()
            {
                return new RuleNode
                    {
                        Name = this.Name,
                        Expression = CloningUtility.CloneObject(this.Expression),
                    };
            }
        }

        public abstract class ExpressionNode
        {
        }

        public class ConcatenationNode : ExpressionNode, ICloneable<ConcatenationNode>
        {
            public ExpressionNode[] Operands { get; set; }

            public ConcatenationNode Clone()
            {
                return new ConcatenationNode
                    {
                        Operands = CloningUtility.CloneArray(this.Operands),
                    };
            }
        }

        public class AlternationNode : ExpressionNode, ICloneable<AlternationNode>
        {
            public ExpressionNode[] Operands { get; set; }

            public AlternationNode Clone()
            {
                return new AlternationNode
                {
                    Operands = CloningUtility.CloneArray(this.Operands),
                };
            }
        }

        public class RuleReferenceNode : ExpressionNode, ICloneable<RuleReferenceNode>
        {
            public string RuleName { get; set; }

            public RuleReferenceNode Clone()
            {
                return new RuleReferenceNode
                {
                    RuleName = this.RuleName,
                };
            }
        }

        public class TokenNode : ExpressionNode, ICloneable<TokenNode>
        {
            public string Token { get; set; }

            public TokenNode Clone()
            {
                return new TokenNode
                {
                    Token = this.Token,
                };
            }
        }

    }
}
