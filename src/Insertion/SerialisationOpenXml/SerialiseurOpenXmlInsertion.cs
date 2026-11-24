using System;
using System.IO;
using System.Text;
using System.Xml;

namespace PowerPointClone.Insertion.SerialisationOpenXml
{
    public class SerialiseurOpenXmlInsertion
    {
        public string GenererXmlDiapositiveInsertion(
            string titreDiapo,
            string texteZoneTexte,
            string typeForme,
            string resumeTableau,
            string cheminImage)
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };

            using (XmlWriter writer = XmlWriter.Create(sb, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("p", "sld", "http://schemas.openxmlformats.org/presentationml/2006/main");
                writer.WriteAttributeString("xmlns", "a", null, "http://schemas.openxmlformats.org/drawingml/2006/main");
                writer.WriteAttributeString("xmlns", "r", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships");

                writer.WriteStartElement("p", "cSld", null);
                writer.WriteAttributeString("name", titreDiapo ?? "Diapositive");
                writer.WriteStartElement("p", "spTree", null);

                if (!string.IsNullOrWhiteSpace(texteZoneTexte))
                {
                    writer.WriteStartElement("p", "sp", null);
                    writer.WriteElementString("a", "t", null, texteZoneTexte);
                    writer.WriteEndElement();
                }

                if (!string.IsNullOrWhiteSpace(typeForme))
                {
                    writer.WriteStartElement("p", "sp", null);
                    writer.WriteAttributeString("shapeType", typeForme);
                    writer.WriteEndElement();
                }

                if (!string.IsNullOrWhiteSpace(resumeTableau))
                {
                    writer.WriteStartElement("p", "tbl", null);
                    writer.WriteElementString("a", "t", null, resumeTableau);
                    writer.WriteEndElement();
                }

                if (!string.IsNullOrWhiteSpace(cheminImage))
                {
                    writer.WriteStartElement("p", "pic", null);
                    writer.WriteAttributeString("source", cheminImage);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement(); // p:spTree
                writer.WriteEndElement(); // p:cSld
                writer.WriteEndElement(); // p:sld
                writer.WriteEndDocument();
            }

            return sb.ToString();
        }

        public void SauvegarderXml(string cheminFichier, string contenuXml)
        {
            if (string.IsNullOrWhiteSpace(cheminFichier))
                throw new ArgumentException("Le chemin de sauvegarde est invalide.");

            string dossier = Path.GetDirectoryName(cheminFichier);
            if (!string.IsNullOrEmpty(dossier) && !Directory.Exists(dossier))
            {
                Directory.CreateDirectory(dossier);
            }

            File.WriteAllText(cheminFichier, contenuXml ?? string.Empty, Encoding.UTF8);
        }
    }
}