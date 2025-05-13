
using static Returns.Helpers.ReturnAnalysisHelper;

namespace Returns.Helpers.Interfaces
{
}
   /* public class ConsistencyReport
    {
        private readonly ValidationResult _data;
        private readonly string _returnId;
        private readonly string _preparedBy;

        public ConsistencyReport(ValidationResult data, string returnId, string preparedBy)
        => (_data, _returnId, _preparedBy) = (data, returnId, preparedBy);

        public void Compose(IDocumentContainer doc)
        {
            doc.Page(page =>
            {
                page.Margin(40);

                page.Content().Column(col =>
                {
                    // Header  
                    col.Item().Text($"Return {_returnId} – Validation Report")
                               .FontSize(18).Bold();
                    col.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
                    col.Item().Text($"Prepared by: {_preparedBy}");
                    col.Item().PaddingVertical(10).LineHorizontal(1);

                    // Summary  
                    col.Item().Text(_data.IsValid ? "✅ PASS" : "❌ FAIL")
                               .FontSize(14).Bold();

                    // Error table  
                    if (_data.ValidationErrors.Any())
                    {
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(30);   // #  
                                c.RelativeColumn(1);    // Category  
                                c.RelativeColumn(2);    // Description  
                                c.RelativeColumn(3);    // Details  
                            });

                            t.Header(h =>
                            {
                                h.Cell().Text("#").Bold();
                                h.Cell().Text("Category").Bold();
                                h.Cell().Text("Description").Bold();
                                h.Cell().Text("Details").Bold();
                            });

                            int i = 1;
                            foreach (var e in _data.ValidationErrors)
                            {
                                t.Cell().Text((i++).ToString());
                                t.Cell().Text(e.Category);
                                t.Cell().Text(e.Description);
                                t.Cell().Text(string.Join(Environment.NewLine,
                                                e.Details.Select(kv => $"{kv.Key}: {kv.Value}")));
                            }
                        });
                    }
                    else
                    {
                        col.Item().PaddingTop(20)
                                  .Text("All cross-form consistency checks passed.");
                    }
                });

                // Footer with page numbers  
                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber().FontSize(10);
                    x.Span(" / ");
                    x.TotalPages().FontSize(10);
                });
            });
        }
    }
    }*/
